using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EarleCode.Lexing;

namespace EarleCode.Grammar
{

    public class GrammarRule : IEnumerable<IGrammarRuleElement>
    {
        public GrammarRule(string name, bool isStatement, IGrammarRuleElement[] elements)
        {
            Name = name;
            IsStatement = isStatement;
            Elements = elements;
        }

        public GrammarRule(string name, bool isStatement, string rule) : this(name, isStatement, Compile(rule))
        {
        }

        public string Name { get; }
        public bool IsStatement { get; }
        public IGrammarRuleElement[] Elements { get; }

        private static IGrammarRuleElement[] Compile(string rule)
        {
            if(rule == null) throw new ArgumentNullException(nameof(rule));

            var lexer = new Lexer("grammar_rule", rule.Replace('`', '"'));

            var conditions = new List<IGrammarRuleElement>();
            var nextConditionIsOptional = false;
            var add = new Action<IGrammarRuleElement>(element => {
                
                conditions.Add(nextConditionIsOptional ? new GrammarRuleElementOptional(element) : element);
                nextConditionIsOptional = false;
            });

            var types =
                typeof(TokenType).GetEnumValues()
                    .OfType<TokenType>()
                    .Select(value => new KeyValuePair<string, TokenType>(value.ToUpperString(), value))
                    .ToDictionary(p => p.Key, p => p.Value);

            while(lexer.MoveNext())
            {
                switch(lexer.Current.Type)
                {
                case TokenType.Token:
                add(new GrammarRuleElementLiteral(new[] { TokenType.Token }, lexer.Current.Value));
                break;
                case TokenType.Identifier:
                if(lexer.Current.Value == "OPTIONAL")
                {
                    nextConditionIsOptional = true;
                    break;
                }

                TokenType tokenType;
                if(types.TryGetValue(lexer.Current.Value, out tokenType))
                {
                    var tokenTypes = new List<TokenType>(new[] { tokenType });
                    for(;;)
                    {
                        var previous = lexer.Current;
                        if(!lexer.MoveNext())
                        {
                            add(new GrammarRuleElementLiteral(tokenTypes.ToArray(), null));
                            break;
                        }

                        if(!lexer.Current.Is(TokenType.Token, "|"))
                        {
                            lexer.Push(previous);
                            add(new GrammarRuleElementLiteral(tokenTypes.ToArray(), null));
                            break;
                        }

                        if(!lexer.MoveNext())
                        {
                            add(new GrammarRuleElementLiteral(tokenTypes.ToArray(), null));
                            break;
                        }

                        lexer.AssertToken(TokenType.Identifier);
                        if(types.TryGetValue(lexer.Current.Value, out tokenType))
                            tokenTypes.Add(tokenType);
                        else
                            throw new Exception("Expected token type");
                    }
                    break;
                }

                add(new GrammarRuleElementEmbed(lexer.Current.Value));
                break;
                case TokenType.StringLiteral:
                add(new GrammarRuleElementLiteral(new[] { TokenType.Identifier }, lexer.Current.Value));
                break;
                default:
                throw new Exception("Invalid token");
                }
            }

            return conditions.ToArray();
        }

        public override string ToString()
        {
            return $"{Name} = \"{string.Join(" ", Elements.Select(e => e.ToString()))}\"";
        }

        #region Implementation of IEnumerator<GrammarRuleElement>

        public IEnumerator<IGrammarRuleElement> GetEnumerator()
        {
            return ((IEnumerable<IGrammarRuleElement>)Elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}