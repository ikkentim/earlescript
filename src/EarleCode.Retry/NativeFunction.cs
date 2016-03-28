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
using EarleCode.Values;

namespace EarleCode
{
    public class NativeFunction : EarleFunction
    {
        private readonly Func<EarleValue[], EarleValue> _native;

        public NativeFunction(string name, Func<EarleValue[], EarleValue> native, params string[] parameters)
            : base(null, name, parameters, null)
        {
            _native = native;
        }

        #region Overrides of EarleFunction

        public override RuntimeLoop CreateLoop(Runtime runtime, EarleValue[] arguments)
        {
            if (Parameters.Length < arguments.Length)
                arguments =
                    arguments.Concat(Enumerable.Repeat(EarleValue.Null, Parameters.Length - arguments.Length)).ToArray();
            
            return new NativeRuntimeLoop(runtime, _native, arguments);
        }

        #endregion

        public class NativeRuntimeLoop : RuntimeLoop
        {
            private readonly EarleValue[] _arguments;
            private readonly Func<EarleValue[], EarleValue> _native;

            public NativeRuntimeLoop(Runtime runtime, Func<EarleValue[], EarleValue> native, EarleValue[] arguments)
                : base(runtime, null, null)
            {
                _native = native;
                _arguments = arguments;
            }

            #region Overrides of RuntimeLoop

            public override EarleValue? Run()
            {
                return _native(_arguments);
            }

            #endregion
        }
    }
}