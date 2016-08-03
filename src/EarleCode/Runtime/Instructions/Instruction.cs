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
        protected EarleStackFrameExecutor Frame { get; private set; }

        #region Implementation of IInstruction

        public void Handle(EarleStackFrameExecutor frame)
        {
            Frame = frame;
            Handle();
        }

        #endregion

        protected abstract void Handle();

        protected int GetInt32()
        {
            FastConvert converter = new FastConvert();
            converter.Byte0 = Frame.PCode[Frame.CIP];
            converter.Byte1 = Frame.PCode[Frame.CIP + 1];
            converter.Byte2 = Frame.PCode[Frame.CIP + 2];
            converter.Byte3 = Frame.PCode[Frame.CIP + 3];
            Frame.CIP += 4;

            return converter.Int32;
        }

        protected float GetSingle()
        {
            var value = BitConverter.ToSingle(Frame.PCode, Frame.CIP);
            Frame.CIP += 4;
            return value;
        }

        protected string GetString()
        {
            var start = Frame.CIP;
            var length = 0;
            while(Frame.PCode[Frame.CIP] != 0)
            {
                length++;
                Frame.CIP++;
            }
            Frame.CIP++;
            return Encoding.ASCII.GetString(Frame.PCode, start, length);
        }

        protected EarleValue Pop()
        {
            return Frame.Stack.Pop();
        }

        protected T Pop<T>()
        {
            return Pop().As<T>();
        }

        protected T PopTo<T>()
        {
            return Pop().CastTo<T>();
        }

        protected void Jump(int count)
        {
            Frame.CIP += count;
        }

        protected EarleValue Peek()
        {
            return Frame.Stack.Peek();
        }

        protected void Push(EarleValue item)
        {
            Frame.Stack.Push(item);
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