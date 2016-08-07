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
    public abstract class EarleBaseStackFrameExecutor : IEarleStackFrameExecutor
    {
        public EarleBaseStackFrameExecutor(EarleValue target)
        {
            Target = target;
        }

        public EarleStackFrame Frame { get; protected set; }

        public EarleValue Target { get; }

        public Stack<EarleRuntimeScope> Scopes { get; } = new Stack<EarleRuntimeScope>();

        public Stack<EarleValue> Stack { get; } = new Stack<EarleValue>();

        public int CIP { get; set; }

        public virtual EarleValue GetValue(EarleVariableReference reference)
        {
            if(reference.Name == "self")
                return string.IsNullOrEmpty(reference.File) ? Target : EarleValue.Undefined;

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

        public virtual EarleFunctionCollection GetFunctionReference(string fileName, string functionName)
        {
            return Scopes.Peek().GetFunctionReference(fileName, functionName);
        }

        public abstract EarleValue? Run();

    }
    
}