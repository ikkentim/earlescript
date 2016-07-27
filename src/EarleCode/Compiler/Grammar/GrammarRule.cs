using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode.Compiler.Grammar
{
    internal class GrammarRule : IEnumerable<IGrammarRuleElement>
    {
        public GrammarRule(string name, bool isStatement, IGrammarRuleElement[] elements)
        {
            Name = name;
            IsStatement = isStatement;
            Elements = elements;
        }

        public string Name { get; }

        public bool IsStatement { get; }

        public IGrammarRuleElement[] Elements { get; }

        public override string ToString()
        {
            return $"{Name} = \"{string.Join(" ", Elements.Select(e => e.ToString()))}\"";
        }

        #region Implementation of IEnumerator<GrammarRuleElement>

        public IEnumerator<IGrammarRuleElement> GetEnumerator()
        {
            return ((IEnumerable<IGrammarRuleElement>)Elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}