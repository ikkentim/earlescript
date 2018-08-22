namespace EarleCode.Compiling.Parsing.Grammars.Productions
{
    public static class ProductionRuleExtensions
    {
        /// <summary>
        ///     Returns a value indicating whether the specified rule can be empty.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <returns>A value indicating whether the specified rule can be empty.</returns>
        public static bool CanBeEmpty(this ProductionRule rule)
        {
            return rule.Elements.Length == 0 || 
                   (rule.Elements.Length == 1 && rule.Elements[0].Type == ProductionRuleElementType.TerminalEmpty) ||
                   (rule.Elements.Length == 1 && rule.Elements[0].Type == ProductionRuleElementType.Terminal && rule.Elements[0].Token.IsEmpty);
        }
    }
}