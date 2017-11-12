using System;

namespace EarleCode.Compiling.Parsing
{
    public class ProductionRuleElement<TTokenType> where TTokenType : struct, IConvertible
    {
        public ProductionRuleElementType Type { get; set; }
        public TTokenType Token { get; set; }
        public string Value { get; set; }

        public ProductionRuleElement(ProductionRuleElementType type, TTokenType token, string value)
        {
            Type = type;
            Token = token;
            Value = value;
        }

        public ProductionRuleElement(ProductionRuleElementType type, string value)
        {
            Type = type;
            Value = value;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return Type == ProductionRuleElementType.Rule ? Value : $"{Value}<{Token}>";
        }

        #endregion
    }
}