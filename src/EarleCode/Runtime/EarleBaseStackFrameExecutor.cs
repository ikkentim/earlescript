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

using System.Collections.Generic;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public abstract class EarleBaseStackFrameExecutor : IEarleStackFrameExecutor
    {
        public EarleBaseStackFrameExecutor(EarleValue target)
        {
            Target = target;
        }

        public EarleStackFrame Frame { get; protected set; }

        public EarleValue Target { get; }

        public Stack<IEarleRuntimeScope> Scopes { get; } = new Stack<IEarleRuntimeScope>();

        public Stack<EarleValue> Stack { get; } = new Stack<EarleValue>();

        public int CIP { get; set; }

        public virtual EarleValue GetValue(string name)
        {
            if (name == "self")
                return Target;

            return Scopes.Peek().GetValue(name);
        }

        public virtual bool SetValue(string name, EarleValue value)
        {
            if (name == "self" || name == "thread")
            {
                Frame.Runtime.HandleWarning($"'{name}' cannot be set!");
                return false;
            }

            return Scopes.Peek().SetValue(name, value);
        }

        public virtual EarleFunctionCollection GetFunctionReference(string fileName, string functionName)
        {
            return Scopes.Peek().GetFunctionReference(fileName, functionName);
        }

        /// <summary>
        ///     Runs this frame.
        /// </summary>
        /// <returns>null if the execution did not complete or a value if it did.</returns>
        public abstract EarleValue? Run();
    }
}