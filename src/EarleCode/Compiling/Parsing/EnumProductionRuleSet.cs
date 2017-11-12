using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EarleCode.Compiling.Lexing;
using EarleCode.Utilities;

namespace EarleCode.Compiling.Parsing
{
    public class EnumProductionRuleSet<TProductRuleEnum, TTokenType> : IProductionRuleSet<TTokenType>
        where TProductRuleEnum : struct, IConvertible
        where TTokenType : struct, IConvertible
    {
        private readonly TTokenType _defToken;
        private readonly ProductionRuleSet<TTokenType> _set;

        public EnumProductionRuleSet(TTokenType defToken)
        {
            _defToken = defToken;
            _set = new ProductionRuleSet<TTokenType>();
            Construct();
        }

        private void Construct()
        {
            var tokenTypes = new Dictionary<string, TTokenType>();
            var productionRuleTypes = new Dictionary<string, TProductRuleEnum>();

            foreach (var value in typeof(TProductRuleEnum).GetEnumValues().Cast<TProductRuleEnum>())
                productionRuleTypes[value.ToString(CultureInfo.InvariantCulture)] = value;

            foreach (var value in typeof(TTokenType).GetEnumValues().Cast<TTokenType>())
                tokenTypes[value.ToString(CultureInfo.InvariantCulture)] = value;

            // TODO: Default
            foreach (var value in typeof(TProductRuleEnum).GetEnumValues().Cast<Enum>())
            {
                var attributes = value.GetCustomAttributes<RuleAttribute>().ToArray();

                if (attributes.Length == 0)
                    throw new ProductionRuleSetException($"RuleAttribute missing for {value}.");

                foreach (var attribute in attributes)
                {
                    if (attribute.Rules == null || attribute.Rules.Length == 0 || attribute.Rules.All(string.IsNullOrWhiteSpace))
                        throw new ProductionRuleSetException($"Rule for {value} is empty.");


                    foreach (var rule in attribute.Rules.Where(n => !string.IsNullOrWhiteSpace(n)))
                    {
                        var lexer = new Lexing.Lexer("rule", rule);

                        var elements = new List<ProductionRuleElement<TTokenType>>();
                        while (lexer.MoveNext())
                        {
                            var token = lexer.Current;

                            if (token.Is(TokenType.Identifier))
                            {
                                if (tokenTypes.TryGetValue(token.Value, out var tokenType))
                                {
                                    elements.Add(new ProductionRuleElement<TTokenType>(ProductionRuleElementType.Token, tokenType, null));
                                }
                                else if (productionRuleTypes.ContainsKey(token.Value))
                                {
                                    elements.Add(new ProductionRuleElement<TTokenType>(ProductionRuleElementType.Rule, token.Value));
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }
                            }
                            else
                            {
                                //TODO:_defToken is ugly, some tokens don't work. Fix Lexer and use it here with custom set.
                                elements.Add(new ProductionRuleElement<TTokenType>(ProductionRuleElementType.Token, _defToken, token.Value));
                            }
                        }

                        _set.Add(value.ToString(), new ProductionRule<TTokenType>(value.ToString(), elements.ToArray()));
                    }
                }

                if (_set.Default == null)
                {
                    _set.Default = _set.Get(value.ToString());
                }
            }
        }

        #region Implementation of IProductionRuleSet

        public IEnumerable<ProductionRule<TTokenType>> Default => _set.Default;

        public IEnumerable<ProductionRule<TTokenType>> Get(string symbol)
        {
            return _set.Get(symbol);
        }

        #endregion
    }
}