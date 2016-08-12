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

using System.Runtime.InteropServices;
using System.Text;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
    /// <summary>
    ///     Represents a good base class for Earle instructions.
    /// </summary>
    /// <seealso cref="IInstruction" />
    internal abstract class Instruction : IInstruction
    {
        /// <summary>
        ///     Gets the frame which is running the instruction.
        /// </summary>
        protected EarleStackFrame Frame { get; private set; }

        #region Implementation of IInstruction

        /// <summary>
        ///     This method is invoked when the instruction needs to be run.
        /// </summary>
        /// <param name="frame">The frame which is running the instruction.</param>
        public void Handle(EarleStackFrame frame)
        {
            // Store the frame so instructions can access it.
            Frame = frame;

            // Run the instruction.
            Handle();
        }

        #endregion

        /// <summary>
        ///     This method is invoked when the instruction needs to be run.
        /// </summary>
        protected abstract void Handle();

        private FastConvert GetConvertable()
        {
            var pCode = Frame.Function.PCode;
            var cip = Frame.Executor.CIP;
            var converter = new FastConvert
            {
                Byte0 = pCode[cip],
                Byte1 = pCode[cip + 1],
                Byte2 = pCode[cip + 2],
                Byte3 = pCode[cip + 3]
            };

            Jump(4);

            return converter;
        }

        protected int GetInt32()
        {
            return GetConvertable().Int32;
        }

        protected float GetSingle()
        {
            return GetConvertable().Single;
        }

        protected string GetString()
        {
            var start = Frame.Executor.CIP;
            var pCode = Frame.Function.PCode;
            var length = 0;
            while (pCode[Frame.Executor.CIP] != 0)
            {
                length++;
                Frame.Executor.CIP++;
            }
            Frame.Executor.CIP++;
            return Encoding.ASCII.GetString(pCode, start, length);
        }

        protected EarleValue Pop()
        {
            return Frame.Executor.Stack.Pop();
        }

        protected T Pop<T>()
        {
            return Pop().CastTo<T>();
        }

        protected void Jump(int count)
        {
            Frame.Executor.CIP += count;
        }

        protected EarleValue Peek()
        {
            return Frame.Executor.Stack.Peek();
        }

        protected void Push(EarleValue item)
        {
            Frame.Executor.Stack.Push(item);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FastConvert
        {
            [FieldOffset(0)]
            public byte Byte0;

            [FieldOffset(1)]
            public byte Byte1;

            [FieldOffset(2)]
            public byte Byte2;

            [FieldOffset(3)]
            public byte Byte3;

            [FieldOffset(0)]
            public readonly int Int32;

            [FieldOffset(0)]
            public readonly float Single;
        }
    }
}