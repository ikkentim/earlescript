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
using EarleCode.Instructions;
using EarleCode.Utilities;
using EarleCode.Values;

namespace EarleCode
{
    public class RuntimeLoop : IRuntimeScope
    {
        private static readonly IInstruction[] Instructions;

        static RuntimeLoop()
        {
            var values = typeof (OpCode).GetEnumValues().OfType<OpCode>().ToArray();
            var count = values.Max(v => (byte) v) + 1;
            Instructions = new IInstruction[count];

            foreach (var value in values)
            {
                var attribute = value.GetCustomAttribute<OpCodeAttribute>();

                if (attribute?.InstructionType == null)
                    continue;

                Instructions[(byte) value] = Activator.CreateInstance(attribute.InstructionType) as IInstruction;
            }
        }

        public RuntimeLoop(Runtime runtime, RuntimeScope superScope, byte[] instructions)
            : this(runtime, superScope, instructions, null)
        {
        }

        public RuntimeLoop(Runtime runtime, RuntimeScope superScope, byte[] instructions,
            IDictionary<string, EarleValue> initialLocals)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            Runtime = runtime;
            PCode = instructions;
            Stack = new Stack<EarleValue>();

            Scopes.Push(new RuntimeScope(superScope, initialLocals));
        }

        public byte[] PCode { get; }

        public Runtime Runtime { get; }

        public Stack<RuntimeScope> Scopes { get; } = new Stack<RuntimeScope>();

        public Stack<EarleValue> Stack { get; }

        public int CIP { get; set; }

        public RuntimeLoop SubLoop { get; set; }

        public virtual EarleValue GetValue(EarleVariableReference reference)
        {
            return Scopes.Peek().GetValue(reference);
        }

        public virtual bool SetValue(EarleVariableReference reference, EarleValue value)
        {
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

//                // Debug
//                var debug = CIP - 1;
//                var str = ((OpCode) instructionIdentifier).GetCustomAttribute<OpCodeAttribute>().BuildString(PCode, ref debug);
//                Console.WriteLine("RUNNING " + str);

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
            if (SubLoop != null)
            {
                var result = SubLoop.Run();
                if (result == null)
                    return false;

                Stack.Push(result.Value);
                SubLoop = null;
            }

            return true;
        }
    }
}