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

        /// <summary>
        ///     Gets the convertable for the dword at the CIP.
        /// </summary>
        /// <returns>A <see cref="FastConvert" /> value.</returns>
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

        /// <summary>
        ///     Gets the integer at the CIP.
        /// </summary>
        /// <returns>The integer at the CIP.</returns>
        protected int GetInt32()
        {
            return GetConvertable().Int32;
        }

        /// <summary>
        ///     Gets the float at the CIP.
        /// </summary>
        /// <returns>The float at the CIP.</returns>
        protected float GetSingle()
        {
            return GetConvertable().Single;
        }

        /// <summary>
        ///     Gets the string at the CIP.
        /// </summary>
        /// <returns>The string at the CIP.</returns>
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

        /// <summary>
        ///     Pops a value off the stack.
        /// </summary>
        /// <returns>The popped value.</returns>
        protected EarleValue Pop()
        {
            return Frame.Executor.Stack.Pop();
        }

        /// <summary>
        ///     Pops a value off the stack and casts it to the specified <typeparamref name="T" /> type.
        /// </summary>
        /// <typeparam name="T">The type to cast the popped value to.</typeparam>
        /// <returns>The popped and cast value.</returns>
        protected T Pop<T>()
        {
            return Pop().CastTo<T>();
        }

        /// <summary>
        ///     Moves the CIP the specified number of instructions.
        /// </summary>
        /// <param name="count">The number of instructions the CIP should move.</param>
        protected void Jump(int count)
        {
            Frame.Executor.CIP += count;
        }

        /// <summary>
        ///     Returns the value on the top of the stack.
        /// </summary>
        /// <returns>The value on the top of the stack.</returns>
        protected EarleValue Peek()
        {
            return Frame.Executor.Stack.Peek();
        }

        /// <summary>
        ///     Pushes the specified item ofto the stack.
        /// </summary>
        /// <param name="item">The item to push onto the stack.</param>
        protected void Push(EarleValue item)
        {
            Frame.Executor.Stack.Push(item);
        }

        /// <summary>
        ///     A structure to swiftly convert dword values.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct FastConvert
        {
            /// <summary>
            ///     The 0th byte.
            /// </summary>
            [FieldOffset(0)]
            public byte Byte0;

            /// <summary>
            ///     The 1st byte.
            /// </summary>
            [FieldOffset(1)]
            public byte Byte1;

            /// <summary>
            ///     The 2nd byte.
            /// </summary>
            [FieldOffset(2)]
            public byte Byte2;

            /// <summary>
            ///     The 3rd byte.
            /// </summary>
            [FieldOffset(3)]
            public byte Byte3;

            /// <summary>
            ///     The integer represented by the dword.
            /// </summary>
            [FieldOffset(0)]
            public readonly int Int32;

            /// <summary>
            ///     The float represented by the dword.
            /// </summary>
            [FieldOffset(0)]
            public readonly float Single;
        }
    }
}