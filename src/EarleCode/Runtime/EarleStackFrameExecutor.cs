// EarleCode
// Copyright 2016 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Values;
using EarleCode.Utilities;

namespace EarleCode.Runtime
{
    public class EarleStackFrameExecutor : IRuntimeScope
    {
        private static readonly IInstruction[] Instructions;

        static EarleStackFrameExecutor()
        {
            var values = typeof(OpCode).GetEnumValues().OfType<OpCode>().ToArray();
            var count = values.Max(v => (byte)v) + 1;
            Instructions = new IInstruction[count];

            foreach(var value in values)
            {
                var attribute = value.GetCustomAttribute<OpCodeAttribute>();

                if(attribute?.InstructionType == null)
                    continue;

                Instructions[(byte)value] = Activator.CreateInstance(attribute.InstructionType) as IInstruction;
            }
        }

        public EarleStackFrameExecutor(EarleStackFrame frame, EarleRuntimeScope superScope, byte[] instructions)
            : this(frame, superScope, instructions, null)
        {
        }

        public EarleStackFrameExecutor(EarleStackFrame frame, EarleRuntimeScope superScope, byte[] instructions,
            EarleDictionary initialLocals)
        {
            if(frame == null) throw new ArgumentNullException(nameof(frame));
            Frame = frame;
            PCode = instructions;
            Stack = new Stack<EarleValue>();

            Scopes.Push(new EarleRuntimeScope(superScope, initialLocals));
        }

        public byte[] PCode { get; }

        public EarleStackFrame Frame { get; }

        public Stack<EarleRuntimeScope> Scopes { get; } = new Stack<EarleRuntimeScope>();

        public Stack<EarleValue> Stack { get; }

        public int CIP { get; set; }

        public virtual EarleValue GetValue(EarleVariableReference reference)
        {
            if(reference.Name == "self")
                return string.IsNullOrEmpty(reference.File) ? Frame.Target : EarleValue.Undefined;
            
            return Scopes.Peek().GetValue(reference);
        }

        public virtual bool SetValue(EarleVariableReference reference, EarleValue value)
        {
            if(reference.Name == "self" || reference.Name == "thread")
            {
                Frame.Runtime.HandleWarning($"'{reference.Name}' cannot be set!");
                return false;
            }

            return Scopes.Peek().SetValue(reference, value);
        }

        public virtual EarleValue? Run()
        {
            // If a value is returned, loop is complete, if null is returned, the loop has not yet been completed.

            if (!RunSubLoop())
                return null;

            while (CIP < PCode.Length)
            {
                var instructionIdentifier = PCode[CIP++];
                var instruction = instructionIdentifier >= Instructions.Length
                    ? null
                    : Instructions[instructionIdentifier];

                if (instruction == null)
                    throw new Exception("Unkown opcode " + (OpCode) instructionIdentifier);

                instruction.Handle(this);

                if (!RunSubLoop())
                    return null;
            }

            return Stack.Pop();
        }

        private bool RunSubLoop()
        {
            if (Frame.SubFrame != null)
            {
                var result = Frame.SubFrame.Run();
                if (result == null)
                    return false;

                Stack.Push(result.Value);
                Frame.SubFrame = null;
            }

            return true;
        }
    }
}