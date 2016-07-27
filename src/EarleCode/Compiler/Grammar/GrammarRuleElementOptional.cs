using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiler.Lexing;

namespace EarleCode.Compiler.Grammar
{
    internal class GrammarRuleElementOptional : IGrammarRuleElement
    {
        public GrammarRuleElementOptional(IGrammarRuleElement element)
        {
            if(element == null) throw new ArgumentNullException(nameof(element));
            Element = element;
        }

        public IGrammarRuleElement Element { get; }

        public IEnumerable<ILexer> Match(GrammarProcessor grammar, ILexer lexer)
        {
            return Element.Match(grammar, lexer).Concat(new[] { lexer });
        }

        public override string ToString()
        {
            return $"OPTIONAL {Element}";
        }
    }
    
}