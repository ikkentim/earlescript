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
using EarleCode.Values;

namespace EarleCode
{
    public class EarleFunction
    {
        public EarleFunction(EarleFile file, string name, string[] parameters, byte[] pCode)
        {
            ;
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            File = file;
            Parameters = parameters;
            PCode = pCode;
            Name = name;
        }

        public EarleFile File { get; }

        public byte[] PCode { get; }

        public string Name { get; }

        public string[] Parameters { get; }

        public virtual RuntimeLoop CreateLoop(Runtime runtime, EarleValue[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            var locals = new Dictionary<string, EarleValue>();

            var index = 0;
            foreach (var parameter in Parameters)
            {
                locals[parameter] = index >= arguments.Length ? EarleValue.Null : arguments[index];
                index++;
            }

            return new RuntimeLoop(runtime, File, PCode, locals);
        }

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"{File}::{Name}";
        }

        #endregion
    }
}