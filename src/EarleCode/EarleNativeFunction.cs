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

using System.Linq;
using EarleCode.Values;

namespace EarleCode
{
    public abstract class EarleNativeFunction : EarleFunction
    {
        protected EarleNativeFunction(string name, params string[] parameters) : base(null, name, parameters, null)
        {
        }

        #region Overrides of EarleFunction

        public override RuntimeLoop CreateLoop(Runtime runtime, EarleValue[] arguments)
        {
            if (Parameters.Length < arguments.Length)
                arguments =
                    arguments.Concat(Enumerable.Repeat(EarleValue.Undefined, Parameters.Length - arguments.Length))
                        .ToArray();

            return new NativeRuntimeLoop(runtime, this, arguments);
        }

        #endregion

        protected abstract EarleValue Invoke(EarleValue[] arguments);

        private class NativeRuntimeLoop : RuntimeLoop
        {
            private readonly EarleValue[] _arguments;
            private readonly EarleNativeFunction _native;

            public NativeRuntimeLoop(Runtime runtime, EarleNativeFunction native, EarleValue[] arguments)
                : base(runtime, null, null)
            {
                _native = native;
                _arguments = arguments;
            }

            #region Overrides of RuntimeLoop

            public override EarleValue? Run()
            {
                return _native.Invoke(_arguments);
            }

            #endregion
        }
    }
}