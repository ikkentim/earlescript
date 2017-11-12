using System;
using System.Collections.Generic;

namespace EarleCode.Compiling.Parsing
{
    public class ProductionRuleSet<TTokenType> : IProductionRuleSet<TTokenType> where TTokenType : struct, IConvertible
    {
        private readonly Dictionary<string, List<ProductionRule<TTokenType>>> _productionRules =
            new Dictionary<string, List<ProductionRule<TTokenType>>>();

        public void Add(string symbol, ProductionRule<TTokenType> rule)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            if (_productionRules.TryGetValue(symbol, out var collection))
                collection.Add(rule);
            else
            {
                collection = new List<ProductionRule<TTokenType>> { rule };
                _productionRules[symbol] = collection;
            }
        }

        public void Clear()
        {
            _productionRules.Clear();
        }

        public IEnumerable<ProductionRule<TTokenType>> Default { get; set; }

        public IEnumerable<ProductionRule<TTokenType>> Get(string symbol)
        {
            _productionRules.TryGetValue(symbol, out var result);
            return result;
        }
    }
}