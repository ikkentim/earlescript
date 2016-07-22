using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using EarleCode.Lexing;

namespace EarleCode.GrammarX
{
    public class GrammarX
    {
        private readonly GrammarRuleTable _table = new GrammarRuleTable();

        public void AddRule(GrammarRule rule)
        {
            _table.Add(rule);
        }

        public void AddRule(string name, bool isStatement, string rule)
        {
            AddRule(new GrammarRule(name, isStatement, rule));
        }
        public string GetMatch(ILexer lexer, bool onlyStatements = false)
        {
            foreach(var ruleName in _table.Where(rule => !onlyStatements || rule.IsStatement).Select(r => r.Name).Distinct())
            {
                if(IsMatch(lexer, ruleName))
                {
                    return ruleName;
                }
            }

            return null;
        }

        public bool IsMatch(ILexer lexer, string ruleName)
        {
            // TODO: Optimize any ( don't need to find all results first )
            return GetMatches(lexer, ruleName).Any();
        }

        public IEnumerable<ILexer> GetMatches(ILexer lexer, string ruleName)
        {
            var wrapRules = new List<GrammarRule>();
            var results = new List<ILexer>();

            foreach(var rule in _table.Get(ruleName))
            {
                if(rule.Elements[0] is GrammarRuleElementEmbed && ((GrammarRuleElementEmbed)rule.Elements[0]).RuleName == ruleName)
                {
                    if(rule.Elements.Length > 1)
                        wrapRules.Add(rule);
                }
                else
                {
                    foreach(var match in GetMatches(lexer, rule))
                    {
                        yield return match;
                        results.Add(match);
                    }
                }
            }

            foreach(var start in results)
                foreach(var match in MatchesRecursive(start, wrapRules))
                    yield return match;
        }

        private IEnumerable<ILexer> MatchesRecursive(ILexer lexer, IEnumerable<GrammarRule> rules)
        {
            var last = new List<ILexer>(new[] { lexer });
            for(;;)
            {
                var newLast = new List<ILexer>();

                foreach(var l in last)
                    foreach(var rule in rules)
                        foreach(var match in GetMatches(l, rule, 1))
                        {
                            yield return match;
                            newLast.Add(match);
                        }
                 

                if(!newLast.Any())
                    yield break;
                
                last = newLast;
            }
        }

        private IEnumerable<ILexer> GetMatches(ILexer lexer, GrammarRule rule, int startElement = 0)
        {
            if(lexer.Current == null)
                yield break;
            
            var element = rule.Elements[startElement];

            if(rule.Elements.Length - 1 == startElement)
            { // Last element
                foreach(var match in element.Match(this, lexer))
                    yield return match;

            }
            else
            { // Not last element
                foreach(var match in element.Match(this, lexer))
                {
                    if(match.Current == null)
                        continue;

                    foreach(var match2 in GetMatches(match, rule, startElement + 1))
                        yield return match2;
                }
            }
        }
    }

    public interface IGrammarRuleElement
    {
        IEnumerable<ILexer> Match(GrammarX grammar, ILexer lexer);
    }

    public class GrammarRuleElementOptional : IGrammarRuleElement
    {
        public GrammarRuleElementOptional(IGrammarRuleElement element)
        {
            if(element == null) throw new ArgumentNullException(nameof(element));
            Element = element;
        }

        public IGrammarRuleElement Element { get; }

        public IEnumerable<ILexer> Match(GrammarX grammar, ILexer lexer)
        {
            return Element.Match(grammar, lexer).Concat(new[] { lexer });
        }

        public override string ToString()
        {
            return $"OPTIONAL {Element}";
        }
    }

    public class GrammarRuleElementEmbed : IGrammarRuleElement
    {
        public GrammarRuleElementEmbed(string ruleName) 
        {
            if(ruleName == null) throw new ArgumentNullException(nameof(ruleName));

            RuleName = ruleName;
        }

        public string RuleName { get; }

        public IEnumerable<ILexer> Match(GrammarX grammar, ILexer lexer)
        {
            return grammar.GetMatches(lexer, RuleName);
        }

        public override string ToString()
        {
            return RuleName;
        }
    }

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
        public IEnumerable<ILexer> Match(GrammarX grammar, ILexer lexer)
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

    internal class GrammarRuleTable : IEnumerable<GrammarRule>
    {
        private readonly IEnumerable<GrammarRule> _empty = new GrammarRule[0];
        private readonly Dictionary<string, List<GrammarRule>> _rules =
            new Dictionary<string, List<GrammarRule>>();

        [DebuggerHidden]
        public void Add(GrammarRule rule)
        {
            if(rule == null)
                throw new ArgumentNullException(nameof(rule));
            List<GrammarRule> r;
            if(!_rules.TryGetValue(rule.Name, out r))
                r = _rules[rule.Name] = new List<GrammarRule>();

            r.Add(rule);
        }

        [DebuggerHidden]
        public IEnumerable<GrammarRule> Get(string name)
        {
            List<GrammarRule> r;
            return _rules.TryGetValue(name, out r)
                ? r
                : _empty;
        }

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        [DebuggerHidden]
        public IEnumerator<GrammarRule> GetEnumerator()
        {
            return _rules.Values.SelectMany(v => v).GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

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