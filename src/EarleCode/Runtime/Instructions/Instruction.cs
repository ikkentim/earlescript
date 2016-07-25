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
            var value = BitConverter.ToInt32(Frame.PCode, Frame.CIP);
            Frame.CIP += 4;

            return value;
        }

        protected float GetSingle()
        {
            var value = BitConverter.ToSingle(Frame.PCode, Frame.CIP);
            Frame.CIP += 4;
            return value;
        }

        protected string GetString()
        {
            var value = "";

            while (Frame.PCode[Frame.CIP] != 0)
                value += (char) Frame.PCode[Frame.CIP++];
            Frame.CIP++;

            return value;
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
            return Pop().CastTo<T>(Frame.Frame.Runtime);
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
    }
}