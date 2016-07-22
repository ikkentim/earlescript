using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EarleCode.Lexing;

namespace EarleCode.Grammar
{

    public interface IGrammarRuleElement
    {
        IEnumerable<ILexer> Match(GrammarProcessor grammar, ILexer lexer);
    }
    
}