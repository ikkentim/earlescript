using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Earle.Blocks;
using Earle.Tokens;
using Earle.Variables;

namespace Earle.Parsers
{
    public class ExpressionParser : Parser<Expression>
    {
        private FunctionCallParser _functionCallParser = new FunctionCallParser();

        #region Overrides of Parser<ExpressionParser>

        public override string ParserRule
        {
            get { return null; }
        }

        public override Expression Parse(Block parent, Tokenizer tokenizer)
        {
            var token = tokenizer.Current;

            Expression expression;
            switch (token.Type)
            {
                case TokenType.Identifier:
                    expression = new VariableExpression(parent, token.Value);
                    tokenizer.MoveNext();
                    break;
                case TokenType.NumberLiteral:
                    float value;
                    if (float.TryParse(token.Value, out value))
                        expression = new ValueExpression(parent, new ValueContainer(VarType.Number, value));
                    else
                        throw new ParseException(token, "Failed to parse number");
                    tokenizer.MoveNext();
                    break;
                case TokenType.StringLiteral:
                    expression = new ValueExpression(parent, new ValueContainer(VarType.String, token.Value));
                    tokenizer.MoveNext();
                    break;
                case TokenType.Token:
                    var tokenType = Compiler.Grammar.GetMatch(tokenizer);

                    switch (tokenType)
                    {
                        case "FUNCTION_CALL":
                            expression = _functionCallParser.Parse(parent, tokenizer);
                            break;
                        case "OPERATOR_UNARY":
                            throw new NotImplementedException();
                        default:
                            throw new ParseException(token, "Unexpected token");
                    }
                    break;
                default:
                    throw new ParseException(token, "Unexpected token type");
            }


            token = tokenizer.Current;

            // Check for ops
            if (token.Type == TokenType.Token  && /* token is operator */ !new []{",", ")", ";"}.Contains(token.Value))
            {
                var op = new OperatorExpression(parent)
                {
                    OP = token.Value,
                    Left = expression
                };
                expression.Parent = op;

                tokenizer.MoveNext();
                op.Right = Parse(op, tokenizer);

                return op;
            }

            return expression;
        }

        #endregion
    }

    public class ParseException : CodeException
    {
        public ParseException(Token token, string error) : base(token, error)
        {
        }

        public ParseException(int line, int column, string error) : base(line, column, error)
        {
        }

        public ParseException(string message) : base(message)
        {
        }
    }
}
