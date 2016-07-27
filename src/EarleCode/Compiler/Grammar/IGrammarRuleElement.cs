using System.Collections.Generic;
using EarleCode.Compiler.Lexing;

namespace EarleCode.Compiler.Grammar
{
    internal interface IGrammarRuleElement
    {
        IEnumerable<ILexer> Match(GrammarProcessor grammar, ILexer lexer);
    }
    
}