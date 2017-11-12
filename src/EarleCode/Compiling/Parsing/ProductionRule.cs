using System;
using System.Linq;

namespace EarleCode.Compiling.Parsing
{
    public class ProductionRule<TTokenType> where TTokenType : struct, IConvertible
    {
        public ProductionRule(string name, ProductionRuleElement<TTokenType>[] elements)
        {
            Name = name;
            Elements = elements;
        }

        public string Name { get; set; }
        public ProductionRuleElement<TTokenType>[] Elements { get; set; }

        #region Overrides of Object

        public override string ToString()
        {
            return string.Join(" ", Elements.Select(n => n.ToString()));
        }

        #endregion
    }
}