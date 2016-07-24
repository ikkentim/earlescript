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
        protected EarleRuntimeLoop Loop { get; private set; }

        protected EarleRuntime Runtime => Loop.Runtime;

        #region Implementation of IInstruction

        public void Handle(EarleRuntimeLoop loop)
        {
            Loop = loop;
            Handle();
        }

        #endregion

        protected abstract void Handle();

        protected int GetInt32()
        {
            var value = BitConverter.ToInt32(Loop.PCode, Loop.CIP);
            Loop.CIP += 4;

            return value;
        }

        protected float GetSingle()
        {
            var value = BitConverter.ToSingle(Loop.PCode, Loop.CIP);
            Loop.CIP += 4;
            return value;
        }

        protected string GetString()
        {
            var value = "";

            while (Loop.PCode[Loop.CIP] != 0)
                value += (char) Loop.PCode[Loop.CIP++];
            Loop.CIP++;

            return value;
        }

        protected EarleValue Pop()
        {
            return Loop.Stack.Pop();
        }

        protected T Pop<T>()
        {
            return Pop().As<T>();
        }

        protected T PopTo<T>()
        {
            return Pop().To<T>(Runtime);
        }

        protected void Jump(int count)
        {
            Loop.CIP += count;
        }

        protected EarleValue Peek()
        {
            return Loop.Stack.Peek();
        }

        protected void Push(EarleValue item)
        {
            Loop.Stack.Push(item);
        }
    }
}