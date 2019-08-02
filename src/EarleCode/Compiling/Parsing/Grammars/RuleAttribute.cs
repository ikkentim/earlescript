// EarleCode
// Copyright 2017 Tim Potze
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

namespace EarleCode.Compiling.Parsing.Grammars
{
    /// <summary>
    ///     Specifies which rules are associated with the symbol this attribute has been attached to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class RuleAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RuleAttribute" /> class.
        /// </summary>
        /// <param name="rules">The rules associated with the symbol the attribute is attached to.</param>
        public RuleAttribute(params string[] rules)
        {
            Rules = rules;
        }

        /// <summary>
        ///     Gets the rules associated with the symbol this attribute is attached to.
        /// </summary>
        public string[] Rules { get; }
    }
}