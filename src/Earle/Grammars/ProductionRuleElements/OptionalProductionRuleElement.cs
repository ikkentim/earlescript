// Earle
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
using Earle.Grammars.ProductionRuleElements;
using Earle.Tokens;

namespace Earle.Grammars
{
    public class OptionalProductionRuleElement : IProductionRuleElement
    {
        private readonly IProductionRuleElement _element;

        public OptionalProductionRuleElement(IProductionRuleElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            _element = element;
        }

        #region Implementation of IProductionRuleElement

        public bool Matches(TokenWalker tokenWalker, ICollection<ProductionRule> rules)
        {
            if (tokenWalker.Current == null)
                return true;

            tokenWalker.CreateSession();
            tokenWalker.FlushOrDropSession(_element.Matches(tokenWalker, rules));
            return true;
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("OPTIONAL {0}", _element);
        }

        #endregion
    }
}