// EarleCode
// Copyright 2015 Tim Potze
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
using EarleCode.Values;

namespace EarleCode
{
    public struct InvocationResult
    {
        public static InvocationResult Empty { get; } = new InvocationResult(InvocationState.None, EarleValue.Null, null);

        private InvocationResult(InvocationState state, EarleValue returnValue, IncompleteInvocationResult result)
        {
            if(state == InvocationState.Incomplete && result == null)
                throw new ArgumentNullException(nameof(result));

            State = state;
            ReturnValue = returnValue;
            Result = result;
        }

        public InvocationResult(InvocationState state, EarleValue returnValue) : this(state, returnValue, null)
        {

        }

        public InvocationResult(IncompleteInvocationResult result)
            : this(InvocationState.Incomplete, EarleValue.Null, result)
        {

        }

        public InvocationState State { get; }
        public EarleValue ReturnValue { get; }
        public IncompleteInvocationResult Result { get; }
    }
}