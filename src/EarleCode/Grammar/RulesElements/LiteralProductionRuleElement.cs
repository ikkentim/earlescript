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
using System.Collections.Generic;
using System.Linq;
using EarleCode.Tokens;

namespace EarleCode.Retry.Grammar.RulesElements
{
    public class LiteralProductionRuleElement : IProductionRuleElement
    {
        private readonly TokenType[] _types;
        private readonly string _value;

        public LiteralProductionRuleElement(params TokenType[] types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            _types = types;
        }

        public LiteralProductionRuleElement(string value, params TokenType[] types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            _value = value;
            _types = types;
        }

        #region Implementation of IProductionRuleElement

        public ProductionRuleMatchResult Matches(TokenWalker tokenWalker, IEnumerable<ProductionRule> rules)
        {
            if (tokenWalker.Current == null)
                return ProductionRuleMatchResult.False;

            if (_types.Contains(tokenWalker.Current.Type) && (_value == null || _value == tokenWalker.Current.Value))
            {
                tokenWalker.MoveNext();
                return ProductionRuleMatchResult.True;
            }

            return ProductionRuleMatchResult.False;
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("{0}{2}{1}{2}", string.Join("|", _types), _value, _value != null ? "`" : "");
        }

        #endregion
    }
}