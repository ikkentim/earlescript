using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EarleCode.Compiling.Earle.AST;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing;
using EarleCode.Compiling.Parsing.ParseTree;
using IParser = EarleCode.Compiling.Parsing.IParser;
using Rule = EarleCode.Compiling.Earle.ProductionRuleEnum;

namespace EarleCode.Compiling
{
    public class EarleParser
    {
        private readonly IParser _parser;

        public EarleParser(IParser parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public ProgramFile Parse(Token[] tokens)
        {
            var tree = _parser.Parse(tokens) as IInteriorNode;

            return Parse(tree) as ProgramFile;
        }

        private static bool IsEmpty(INode node)
        {
            return node is ILeafNode leaf && leaf.Token.IsEmpty;
        }

        private static TResult Collect<TNode, TResult>(IInteriorNode node, string collect,
            Func<TNode, TResult, TResult> collector, TResult initial = default(TResult)) where TNode : INode
        {
            var result = initial;
            foreach (var c in node.Children)
            {
                switch (c)
                {
                    case IInteriorNode interior when interior.Rule == collect:
                        Collect(interior, collect, collector, result);
                        break;
                    case TNode t:
                        collector(t, result);
                        break;
                }
            }

            return result;
        }

        private static string ParseValue(INode node)
        {
            return ParseValue(node as ILeafNode);
        }

        private static string ParseValue(ILeafNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return node.Token.Value;
        }

        private static List<string> ParseIdentifiers(INode node)
        {
            return ParseIdentifiers(node as IInteriorNode);
        }

        private static List<string> ParseIdentifiers(IInteriorNode node)
        {
            if (node.Rule == nameof(Rule.IdentifierList))
            {
                return node.Children.Count == 0 || IsEmpty(node.Children[0]) ? null : ParseIdentifiers(node.Children[0]);
            }

            if (node.Rule == nameof(Rule.IdentifierListCont))
            {
                if (node.Children.Count == 1)
                    return new List<string> {ParseValue(node.Children[0])};
                var next = ParseIdentifiers(node.Children[0]);
                next.Add(ParseValue(node.Children[2]));
                return next;
            }

            throw new ParserException();
        }

        private static string ParsePath(INode node)
        {
            return ParsePath(node as IInteriorNode);
        }

        private static string ParsePath(IInteriorNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (node.Rule == nameof(Rule.Path))
                return "\\" + ParsePath(node.Children.Last());
            if (node.Rule == nameof(Rule.PathCont))
                if (node.Children.Count == 1)
                    return ParseValue(node.Children[0]);
                else if (node.Children.Count == 3)
                    return ParsePath(node.Children[0]) + "\\" + ParseValue(node.Children[2]);

            throw new ParserException();
        }

        private static IReadOnlyList<Expression> ParseExpressionList(INode node)
        {
            return ParseExpressionList(node as IInteriorNode);
        }

        private static IReadOnlyList<Expression> ParseExpressionList(IInteriorNode node)
        {
            return node.Children.Count > 0 && node.Children[0] is IInteriorNode child
                ? Collect(child, nameof(Rule.ExpressionListCont),
                    (IInteriorNode check, List<Expression> prev) =>
                    {
                        if (check.Rule == nameof(Rule.Expression))
                            prev.Add(ParseExpression(check));

                        return prev;
                    },
                    new List<Expression>())
                : null;
        }

        private static FilePosition GetFilePosition(INode node)
        {
            switch (node)
            {
                case ILeafNode leaf:
                    return leaf.Token.Position;
                case IInteriorNode interior:
                    return GetFilePosition(interior.Children[0]);
                default:
                    throw new ParserException();
            }
        }

        private static IASTNode Parse(INode node)
        {
            return Parse(node as IInteriorNode);
        }

        private static IASTNode Parse(IInteriorNode node)
        {
            if (node.Children.Count == 0) node = new InteriorNode(node.Rule, new[] {new LeafNode(Token.Empty),}); // temp fix

            switch (node.Rule)
            {
                case nameof(Rule.File):
                    return Parse(node.Children[0]);
                case nameof(Rule.FileCont):
                    var addition = Parse(node.Children.Last());

                    List<Include> includes;
                    List<FunctionDeclaration> functionDeclarations;
                    ProgramFile file;
                    if (node.Children.Count == 1)
                    {
                        includes = new List<Include>();
                        functionDeclarations = new List<FunctionDeclaration>();
                        file = new ProgramFile(GetFilePosition(node), includes, functionDeclarations);
                    }
                    else
                    {
                        file = Parse(node.Children[0]) as ProgramFile;

                        Debug.Assert(file != null, nameof(file) + " != null");

                        includes = file.Includes as List<Include>;
                        functionDeclarations = file.FunctionDeclarations as List<FunctionDeclaration>;
                    }

                    Debug.Assert(includes != null, nameof(includes) + " != null");
                    Debug.Assert(functionDeclarations != null, nameof(functionDeclarations) + " != null");

                    switch (addition)
                    {
                        case Include include:
                            includes.Add(include);
                            break;
                        case FunctionDeclaration functionDeclaration:
                            functionDeclarations.Add(functionDeclaration);
                            break;
                    }

                    return file;
                case nameof(Rule.Include):
                    return new Include(GetFilePosition(node), ParsePath(node.Children[2]));
                case nameof(Rule.FunctionDeclaration):
                    var name = ParseValue(node.Children[0]);
                    var parameters = ParseIdentifiers(node.Children[2]);
                    var statement = Parse(node.Children[4]) as Statement;
                    return new FunctionDeclaration(GetFilePosition(node), name, parameters, new[] {statement});
                case nameof(Rule.Statement):
                    return Parse(node.Children[0]);
                case nameof(Rule.StatementBlock):
                    var statements = Collect(node.Children[1] as IInteriorNode, nameof(Rule.StatementsCont),
                        (IInteriorNode n, List<Statement> prev) =>
                        {
                            prev.Add(Parse(n.Children.Last()) as Statement);
                            return prev;
                        }, new List<Statement>());
                    return new StatementBlock(GetFilePosition(node), statements);
                case nameof(Rule.StatementFunctionCall):
                    return new StatementFunctionCall(GetFilePosition(node), Parse(node.Children[0]) as FunctionCall);
                case nameof(Rule.StatementBreak):
                    return new StatementBreak(GetFilePosition(node));
                case nameof(Rule.StatementContinue):
                    return new StatementContinue(GetFilePosition(node));
                case nameof(Rule.StatementReturn):
                    return new StatementReturn(GetFilePosition(node), node.Children.Count == 3 ? ParseExpression(node.Children[1]) : null);
                case nameof(Rule.FunctionCall):
                    return Parse(node.Children[0]);
                case nameof(Rule.TargetlessFunctionCall):
                    return new FunctionCall(GetFilePosition(node), Parse(node.Children[0]) as FunctionIdentifier,
                        ParseExpressionList(node.Children[2]), false, null);
                case nameof(Rule.TargetedFunctionCall):
                    return new FunctionCall(GetFilePosition(node), Parse(node.Children[1]) as FunctionIdentifier,
                        ParseExpressionList(node.Children[3]), false,
                        ParseExpression(node.Children[0]));
                case nameof(Rule.TargetlessThreadedFunctionCall):
                    return new FunctionCall(GetFilePosition(node), Parse(node.Children[1]) as FunctionIdentifier,
                        ParseExpressionList(node.Children[3]), true, null);
                case nameof(Rule.TargetedThreadedFunctionCall):
                    return new FunctionCall(GetFilePosition(node), Parse(node.Children[2]) as FunctionIdentifier,
                        ParseExpressionList(node.Children[4]), true,
                        ParseExpression(node.Children[0]));
                case nameof(Rule.FunctionIdentifier) when node.Children[0] is ILeafNode leaf:
                    return new ImplicitFunctionIdentifier(GetFilePosition(node), ParseValue(leaf));
                case nameof(Rule.FunctionIdentifier):
                    return Parse(node.Children[0]);
                case nameof(Rule.ExplicitFunctionIdentifier):
                    return new ExplicitFunctionIdentifier(GetFilePosition(node), node.Children.Count == 3 ? ParsePath(node.Children[0]) : null,
                        ParseValue(node.Children.Last()));
                case nameof(Rule.UnboxedFunctionIdentifier):
                    return new UnboxedFunctionIdentifier(GetFilePosition(node), ParseExpression(node.Children[2]));
                case nameof(Rule.StatementIf):
                    return new StatementIf(GetFilePosition(node), ParseExpression(node.Children[2]),
                        new List<Statement> {Parse(node.Children[4]) as Statement}, node.Children.Count < 7 ? null : new List<Statement> {Parse(node.Children[6]) as Statement});
                case nameof(Rule.StatementWhile):
                    return new StatementWhile(GetFilePosition(node), ParseExpression(node.Children[2]),
                        new List<Statement> {Parse(node.Children[4]) as Statement});
                case nameof(Rule.StatementDoWhile):
                    return new StatementDoWhile(GetFilePosition(node), ParseExpression(node.Children[4]),
                        new List<Statement> {Parse(node.Children[1]) as Statement});
                case nameof(Rule.StatementWait):
                    return new StatementWait(GetFilePosition(node), ParseExpression(node.Children[1]));
                case nameof(Rule.StatementAssignments):
                    return new StatementAssignments(GetFilePosition(node), ParseAssignments(node.Children[0] as IInteriorNode));
                case nameof(Rule.StatementFor):
                    var assignmentsNode = node.Children[2] as IInteriorNode;
                    var conditionNode = node.Children[4] as IInteriorNode;
                    var incrementsNode = node.Children[6] as IInteriorNode;

                    IReadOnlyList<Expression> assignments = null;
                    Expression condition = null;
                    IReadOnlyList<Expression> increments = null;

                    Debug.Assert(assignmentsNode != null, nameof(assignmentsNode) + " != null");
                    Debug.Assert(conditionNode != null, nameof(conditionNode) + " != null");
                    Debug.Assert(incrementsNode != null, nameof(incrementsNode) + " != null");

                    if (assignmentsNode.Children.Count > 0 && assignmentsNode.Children[0] is IInteriorNode assignmentsNodeChild)
                        assignments = ParseAssignments(assignmentsNodeChild.Children[0] as IInteriorNode);
                    if (conditionNode.Children.Count > 0 && conditionNode.Children[0] is IInteriorNode conditionNodeChild)
                        condition = ParseExpression(conditionNodeChild);
                    if (incrementsNode.Children.Count > 0 && incrementsNode.Children[0] is IInteriorNode incrementsNodeChild)
                        increments = ParseAssignments(incrementsNodeChild);

                    return new StatementFor(GetFilePosition(node), assignments, condition, increments,
                        new List<Statement> {Parse(node.Children[8] as IInteriorNode) as Statement});
                default:
                    throw new ParserException("No parser for " + node.Rule);
            }
        }

        private static Expression ParseAssignment(INode node)
        {
            return ParseAssignment(node as IInteriorNode);
        }

        private static Expression ParseAssignment(IInteriorNode node)
        {
            switch (node.Rule)
            {
                case nameof(Rule.Assignment):
                    return ParseAssignment(node.Children[0]);
                case nameof(Rule.VariableAssignment):
                    return new AssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.OrAssignment):
                    return new OrAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.AndAssignment):
                    return new AndAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.XorAssignment):
                    return new XorAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.LeftShiftAssignment):
                    return new LeftShiftAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.RightShiftAssignment):
                    return new RightShiftAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.AdditionAssignment):
                    return new AdditionAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.SubtractionAssignment):
                    return new SubtractionAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.MultiplicationAssignment):
                    return new MultiplicationAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.DivisionAssignment):
                    return new DivisionAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.ModuloAssignment):
                    return new ModuloAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.PostfixAdditionAssignment):
                    return new PostfixAdditionAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]));
                case nameof(Rule.PrefixAdditionAssignment):
                    return new PrefixAdditionAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[1]));
                case nameof(Rule.PostfixSubtractionAssignment):
                    return new PostfixSubtractionAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[0]));
                case nameof(Rule.PrefixSubtractionAssignment):
                    return new PrefixSubtractionAssignmentExpression(GetFilePosition(node), ParseVariable(node.Children[1]));
                default:
                    throw new ParserException();
            }
        }

        private static IReadOnlyList<Expression> ParseAssignments(IInteriorNode node)
        {
            return Collect(node, nameof(Rule.Assignments),
                (IInteriorNode node1, List<Expression> prev) =>
                {
                    prev.Add(ParseAssignment(node1));

                    return prev;
                },
                new List<Expression>());
        }

        private static bool IsExpression(IInteriorNode node)
        {
            if (node == null)
                return false;

            switch (node.Rule)
            {
                case nameof(Rule.Expression):
                case nameof(Rule.Expression2):
                case nameof(Rule.Expression3):
                case nameof(Rule.Expression4):
                case nameof(Rule.Expression5):
                case nameof(Rule.Expression6):
                case nameof(Rule.Expression7):
                case nameof(Rule.Expression8):
                case nameof(Rule.Expression9):
                case nameof(Rule.Expression10):
                case nameof(Rule.Expression11):
                case nameof(Rule.Expression12):
                    return true;
                default:
                    return false;
            }
        }

        private static Expression ParseExpression(INode node)
        {
            return ParseExpression(node as IInteriorNode);
        }

        private static Expression ParseExpression(IInteriorNode node)
        {
            if (node.Children.Count == 1 && node.Children[0] is IInteriorNode child && IsExpression(child))
            {
                return ParseExpression(child);
            }

            switch (node.Rule)
            {
                case nameof(Rule.Expression):
                    return ParseAssignment(node.Children[0]);
                case nameof(Rule.Expression2):
                    return new OrExpression(GetFilePosition(node), ParseExpression(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.Expression3):
                    return new AndExpression(GetFilePosition(node), ParseExpression(node.Children[0]), ParseExpression(node.Children[2]));
                case nameof(Rule.Expression4):
                    return new BitwiseOrExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                        ParseExpression(node.Children[2]));
                case nameof(Rule.Expression5):
                    return new BitwiseXorExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                        ParseExpression(node.Children[2]));
                case nameof(Rule.Expression6):
                    return new BitwiseAndExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                        ParseExpression(node.Children[2]));
                case nameof(Rule.Expression7):
                    switch (ParseValue(node.Children[1]))
                    {
                        case "==":
                            return new EqualExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        case "!=":
                            return new NotEqualExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        default:
                            throw new ParserException();
                    }
                case nameof(Rule.Expression8):
                    switch (ParseValue(node.Children[1]))
                    {
                        case "<":
                            return new LessThanExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        case ">":
                            return new GreaterThanExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        case "<=":
                            return new LessOrEqualExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        case ">=":
                            return new GreaterOrEqualExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        default:
                            throw new ParserException();
                    }
                case nameof(Rule.Expression9):
                    switch (ParseValue(node.Children[1]))
                    {
                        case "<<":
                            return new ShiftLeftExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        case ">>":
                            return new ShiftRightExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        default:
                            throw new ParserException();
                    }
                case nameof(Rule.Expression10):
                    switch (ParseValue(node.Children[1]))
                    {
                        case "+":
                            return new AddExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        case "-":
                            return new SubtractExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        default:
                            throw new ParserException();
                    }
                case nameof(Rule.Expression11):
                    switch (ParseValue(node.Children[1]))
                    {
                        case "*":
                            return new MultiplyExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        case "/":
                            return new DivideExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        case "%":
                            return new ModuloExpression(GetFilePosition(node), ParseExpression(node.Children[0]),
                                ParseExpression(node.Children[2]));
                        default:
                            throw new ParserException();
                    }
                case nameof(Rule.Expression12) when node.Children.Count == 3:
                    return ParseExpression(node.Children[1]);
                case nameof(Rule.Expression12):
                    return ParseExpression(node.Children[0]);
                case nameof(Rule.ExplicitFunctionIdentifier):
                    return new ExplicitFunctionIdentifierExpression(GetFilePosition(node), Parse(node) as ExplicitFunctionIdentifier);
                case nameof(Rule.Value):
                    if (node.Children[0] is ILeafNode leafExpression)
                        switch (leafExpression.Token.Type)
                        {
                            case TokenType.NumberLiteral:
                                if (int.TryParse(leafExpression.Token.Value, out var intValue))
                                    return new NumberExpression(GetFilePosition(node), intValue);
                                else if (float.TryParse(leafExpression.Token.Value, out var floatValue))
                                    return new NumberExpression(GetFilePosition(node), floatValue);
                                else
                                    throw new ParserException();
                            case TokenType.StringLiteral:

                                return new StringExpression(GetFilePosition(node), leafExpression.Token.Value
                                    .Substring(1, leafExpression.Token.Value.Length - 2).Replace("\\\"", "\""));
                            default:
                                throw new ParserException();
                        }
                    return ParseExpression(node.Children[0]);
                case nameof(Rule.Variable):
                    return new VariableExpression(GetFilePosition(node), ParseVariable(node));
                case nameof(Rule.FunctionCall):
                    return Parse(node) as FunctionCall;
                default:
                    throw new ParserException("Unsupported node " + node.Rule + ": " + node);
            }
        }

        private static Variable ParseVariable(INode node)
        {
            return ParseVariable(node as IInteriorNode);
        }

        private static Variable ParseVariable(IInteriorNode node)
        {
            return new Variable(GetFilePosition(node), ParseValue(node.Children[0]));
        }
    }
}