using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EarleCode.Lexing;

namespace EarleCode.Grammar
{
    public class GrammarProcessor
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
}