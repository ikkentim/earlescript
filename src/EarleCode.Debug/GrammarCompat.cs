using EarleCode.Compiling.Parsing.Grammars;

namespace EarleCode.Debug
{
    /// <summary>
    /// Grammar for compatibility testing. Should be widely acceptable grammar.
    /// </summary>
    public enum GrammarCompat
    {
        [Rule("A `a`")]
        S,
        [Rule("B D")]
        A,
        [Rule("`b`", "")]
        B,
        [Rule("`d`", "")]
        D
    }
}