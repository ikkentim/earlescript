using System;
using System.Collections.Generic;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.AST;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    ///     Represents a recursive top-down back-tracking parser.
    /// </summary>
    public class Parser : IParser<TokenType>
    {
        public Parser(IProductionRuleSet<TokenType> productionRules)
        {
            ProductionRules = productionRules ?? throw new ArgumentNullException(nameof(productionRules));
        }

        public IProductionRuleSet<TokenType> ProductionRules { get; }

        private struct ParseInfo
        {
            public bool Success;
            public ILexer<TokenType> Lexer;
            public IAbstractSyntaxTreeNode Node;
            public int RuleIndex;

            public static readonly ParseInfo Empty = new ParseInfo();
        }

        private struct BackTrackInfo
        {
            public ILexer<TokenType> Lexer;
            public int Location;
            public int RuleIndex;
        }

        private ParseInfo Parse(ProductionRule<TokenType> rule, ILexer<TokenType> lexer, IEnumerable<IAbstractSyntaxTreeNode> seed =null)
        {
            if (lexer.Current.Value == null)
                return ParseInfo.Empty;

            lexer = (ILexer<TokenType>) lexer.Clone();

            var children = new List<IAbstractSyntaxTreeNode>();
            var locations = new Stack<BackTrackInfo>();

            if (seed != null) children.AddRange(seed);
            
            var startLocation = 0;
            for (var i = children.Count; i < rule.Elements.Length; i++)
            {
                bool BackTrack()
                {
                    if (locations.Count == 0)
                        return false;

                    var info = locations.Pop();
                    children.RemoveRange(info.Location, children.Count - info.Location);
                    i = info.Location - 1;
                    lexer = info.Lexer;
                    startLocation = info.RuleIndex + 1;
                    return true;
                }

                var el = rule.Elements[i];

                if (lexer.Current.Value == null)
                    return ParseInfo.Empty;

                switch (el.Type)
                {
                    case ProductionRuleElementType.Rule:
                        var result = Parse(ProductionRules.Get(el.Value), lexer, startLocation);

                        if (!result.Success)
                        {
                            if (BackTrack())
                                continue;
                            return ParseInfo.Empty;
                        }

                        children.Add(result.Node);
                        locations.Push(new BackTrackInfo
                        {
                            Lexer = lexer,
                            Location = i,
                            RuleIndex = result.RuleIndex
                        });

                        lexer = result.Lexer;
                        break;
                    case ProductionRuleElementType.Token:
                        if ((el.Value == null && lexer.Current.Is(el.Token)) || lexer.Current.Is(el.Token, el.Value))
                        {
                            children.Add(new AbstractSyntaxTreeLeafNode<TokenType>(lexer.Current));
                            lexer.MoveNext();

                            break;
                        }
                        else
                        {
                            if (BackTrack())
                                continue;
                            return ParseInfo.Empty;
                        }
                }

                startLocation = 0;
            }

            IAbstractSyntaxTreeNode node = new AbstractSyntaxTreeInteriorNode(rule.Name, children);

            // Be greedy
            if (rule.Elements[0].Type == ProductionRuleElementType.Rule && rule.Elements[0].Value == rule.Name)
            {
                ParseInfo parseInfo;
                do
                {
                    parseInfo = Parse(rule, lexer, new[] { node });
                    if (parseInfo.Success)
                    {
                        lexer = parseInfo.Lexer;
                        node = parseInfo.Node;
                    }
                } while (parseInfo.Success);

            }
            return new ParseInfo
            {
                Lexer = lexer,
                Node = new AbstractSyntaxTreeInteriorNode(rule.Name, children),
                Success = true
            };
        }

        private ParseInfo Parse(IEnumerable<ProductionRule<TokenType>> rules,
            ILexer<TokenType> lexer, int startAtOption = 0)
        {
            var i = 0;
            foreach (var rule in rules)
            {
                if (i < startAtOption)
                {
                    i++;
                    continue;
                }

                var result = Parse(rule, lexer);
                if (result.Success)
                {
                    result.RuleIndex = i;
                    return result;
                }

                i++;
            }

            return ParseInfo.Empty;
        }

        private IAbstractSyntaxTreeNode ParseRoot(ILexer<TokenType> lexer)
        {
            return !lexer.MoveNext()
                ? null
                : Parse(ProductionRules.Default, lexer).Node;
        }

        #region Implementation of IParser<TokenType>

        public IAbstractSyntaxTreeNode Parse(ILexer<TokenType> lexer)
        {
            return ParseRoot(lexer);
        }

        #endregion
    }
}