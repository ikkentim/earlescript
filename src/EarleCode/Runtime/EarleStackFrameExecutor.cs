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
using System.Linq;
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Values;
using EarleCode.Utilities;

namespace EarleCode.Runtime
{
    public sealed class EarleStackFrameExecutor : EarleBaseStackFrameExecutor
    {
        private static readonly IInstruction[] Instructions;

        static EarleStackFrameExecutor()
        {
            // Build up array of instructions
            var ops = typeof(OpCode).GetEnumValues().OfType<OpCode>().ToArray();
            var count = ops.Max(v => (byte)v) + 1;
            Instructions = new IInstruction[count];

            foreach(var op in ops)
            {
                var attribute = op.GetCustomAttribute<OpCodeAttribute>();

                if(attribute?.InstructionType == null)
                    continue;

                Instructions[(byte)op] = attribute.CreateInstruction(op);
            }
        }

        public EarleStackFrameExecutor(EarleStackFrame frame, IEarleRuntimeScope superScope)
            : this(frame, superScope, null)
        {
        }

        public EarleStackFrameExecutor(EarleStackFrame frame, IEarleRuntimeScope superScope, EarleDictionary locals) : base(frame)
        {
            Frame.Scopes.Push(new EarleRuntimeScope(superScope, locals));
        }

        public override EarleValue? Run()
        {
            // If a value is returned, loop is complete, if null is returned, the loop has not yet been completed.

            if (!Frame.Thread.IsAlive || !RunSubFrame())
                return null;

            var frame = Frame;
            var pCode = frame.Function.PCode;
            while (frame.CIP < pCode.Length)
            {
                if(!frame.Thread.IsAlive)
                    return null;
                
                var instructionIdentifier = pCode[frame.CIP++];
                var instruction = instructionIdentifier >= Instructions.Length
                    ? null
                    : Instructions[instructionIdentifier];

                if (instruction == null)
                    throw new Exception("Unkown opcode " + (OpCode) instructionIdentifier);

                instruction.Handle(this);

                if (!RunSubFrame())
                    return null;
            }

            return frame.Stack.Pop();
        }

        private bool RunSubFrame()
        {
            if (Frame.SubFrame != null)
            {
                var result = Frame.SubFrame.Run();
                if (result == null)
                    return false;

                Frame.Stack.Push(result.Value);
                Frame.SubFrame = null;
            }

            return true;
        }
    }
}