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
using System.Runtime.InteropServices;
using System.Text;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
    internal abstract class Instruction : IInstruction
    {
        protected EarleStackFrameExecutor Executor { get; private set; }

        #region Implementation of IInstruction

        public void Handle(EarleStackFrameExecutor executor)
        {
            Executor = executor;
            Handle();
        }

        #endregion

        protected abstract void Handle();

        private FastConvert GetConvertable()
        {
            var pCode = Executor.Frame.Function.PCode;
            var cip = Executor.Frame.CIP;
            FastConvert converter = new FastConvert();
            converter.Byte0 = pCode[cip];
            converter.Byte1 = pCode[cip + 1];
            converter.Byte2 = pCode[cip + 2];
            converter.Byte3 = pCode[cip + 3];

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
            var start = Executor.Frame.CIP;
            var pCode = Executor.Frame.Function.PCode;
            var length = 0;
            while(pCode[Executor.Frame.CIP] != 0)
            {
                length++;
                Executor.Frame.CIP++;
            }
            Executor.Frame.CIP++;
            return Encoding.ASCII.GetString(pCode, start, length);
        }

        protected EarleValue Pop()
        {
            return Executor.Frame.Stack.Pop();
        }

        protected T Pop<T>()
        {
            return Pop().CastTo<T>();
        }

        protected void Jump(int count)
        {
            Executor.Frame.CIP += count;
        }

        protected EarleValue Peek()
        {
            return Executor.Frame.Stack.Peek();
        }

        protected void Push(EarleValue item)
        {
            Executor.Frame.Stack.Push(item);
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
            public int Int32;
            [FieldOffset(0)]
            public float Single;
        }
    }
}