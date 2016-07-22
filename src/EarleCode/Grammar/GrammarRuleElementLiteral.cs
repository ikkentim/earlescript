using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EarleCode.Lexing;

namespace EarleCode.Grammar
{

    public class GrammarRuleElementLiteral : IGrammarRuleElement
    {
        public GrammarRuleElementLiteral(TokenType[] tokenTypes, string tokenValue)
        {
            if(tokenTypes == null) throw new ArgumentNullException(nameof(tokenTypes));
            TokenTypes = tokenTypes;
            TokenValue = tokenValue;
        }

        TokenType[] TokenTypes { get; }
        string TokenValue { get; }

        [DebuggerHidden]
        public IEnumerable<ILexer> Match(GrammarProcessor grammar, ILexer lexer)
        {
            foreach(var tokenType in TokenTypes)
            {
                if(TokenValue == null ? lexer.Current.Is(tokenType) : lexer.Current.Is(tokenType, TokenValue))
                {
                    var newLexer = lexer.Clone();
                    newLexer.MoveNext();
                    yield return newLexer;
                }
            }
        }

        public override string ToString()
        {
            if(TokenValue == null)
                return string.Join("|", TokenTypes.Select(t => t.ToUpperString()));
            return $"`{TokenValue}`";
        }
    }
    
}