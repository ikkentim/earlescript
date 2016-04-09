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
using EarleCode.Lexing;

namespace EarleCode.Grammar
{
    internal class LiteralRuleElement : IRuleElement
    {
        private readonly TokenType[] _types;
        private readonly string _value;

        public LiteralRuleElement(TokenType[] types, string value)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            _types = types;
            _value = value;
        }

        #region Implementation of IRuleElement

        public IEnumerable<ILexer> GetMatches(ILexer lexer, ProductionRuleTable rules)
        {
            var isMatch = lexer.Current != null && (_value == null || _value == lexer.Current.Value) &&
                          _types.Contains(lexer.Current.Type);

            if (isMatch)
            {
                var clone = lexer.Clone();
                clone.MoveNext();
                return new[] {clone};
            }

            return new ILexer[0];
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _value ?? string.Join("|", _types);
        }

        #endregion
    }
}