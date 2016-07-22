using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EarleCode.Lexing;

namespace EarleCode.Grammar
{

    public class GrammarRuleElementEmbed : IGrammarRuleElement
    {
        public GrammarRuleElementEmbed(string ruleName) 
        {
            if(ruleName == null) throw new ArgumentNullException(nameof(ruleName));

            RuleName = ruleName;
        }

        public string RuleName { get; }

        public IEnumerable<ILexer> Match(GrammarProcessor grammar, ILexer lexer)
        {
            return grammar.GetMatches(lexer, RuleName);
        }

        public override string ToString()
        {
            return RuleName;
        }
    }
    
}