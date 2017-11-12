using System;
using System.Collections.Generic;

namespace EarleCode.Compiling.Parsing
{
    public interface IProductionRuleSet<TTokenType> where TTokenType : struct, IConvertible
    {
        IEnumerable<ProductionRule<TTokenType>> Default { get; }
        IEnumerable<ProductionRule<TTokenType>> Get(string symbol);
    }
}