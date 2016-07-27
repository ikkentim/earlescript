using EarleCode.Compiler.Lexing;

namespace EarleCode.Compiler.Grammar
{
    public interface IGrammarProcessor
    {
        void AddRule(string name, bool isStatement, string rule);
        string GetMatch(ILexer lexer, bool onlyStatements = false);
        bool IsMatch(ILexer lexer, string ruleName);
    }
}