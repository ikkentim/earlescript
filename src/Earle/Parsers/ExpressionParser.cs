using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Earle.Blocks;
using Earle.Blocks.Expressions;
using Earle.Tokens;
using Earle.Variables;

namespace Earle.Parsers
{
    public class ExpressionParser : Parser<Expression>
    {
        private readonly FunctionCallParser _functionCallParser = new FunctionCallParser();

        #region Overrides of Parser<ExpressionParser>

        public override string ParserRule
        {
            get { return null; }
        }

        public override Expression Parse(Block parent, Tokenizer tokenizer)
        {
            Expression expression;
            switch (tokenizer.Current.Type)
            {
                case TokenType.Identifier:
                    expression = new VariableExpression(parent, tokenizer.Current.Value);
                    MoveNext(tokenizer);
                    break;
                case TokenType.NumberLiteral:
                    float fValue;
                    int iValue;
                    if (int.TryParse(tokenizer.Current.Value, out iValue))
                        expression = new ValueExpression(parent, new ValueContainer(VarType.Integer, iValue));
                    else if (float.TryParse(tokenizer.Current.Value, out fValue))
                        expression = new ValueExpression(parent, new ValueContainer(VarType.Float, fValue));
                    else
                        throw new ParseException(tokenizer.Current, "Failed to parse number");
                    MoveNext(tokenizer);
                    break;
                case TokenType.StringLiteral:
                    expression = new ValueExpression(parent, new ValueContainer(VarType.String, tokenizer.Current.Value));
                    MoveNext(tokenizer);
                    break;
                case TokenType.Token:
                    if (Compiler.Grammar.Matches(tokenizer, "FUNCTION_CALL"))
                        expression = _functionCallParser.Parse(parent, tokenizer);
                    else if (Compiler.Grammar.Matches(tokenizer, "OPERATOR_UNARY"))
                    {
                        var unaryop = tokenizer.Current.Value;
                        MoveNext(tokenizer);
                        expression = new UnaryOperatorExpression(parent, unaryop, Parse(parent, tokenizer));
                    }
                    else
                        throw new ParseException(tokenizer.Current, "Unexpected token");
                    break;
                default:
                    throw new ParseException(tokenizer.Current, "Unexpected token type");
            }

            // Check for ops
            if (tokenizer.Current.Type == TokenType.Token && /* token is operator */
                !new[] {",", ")", ";"}.Contains(tokenizer.Current.Value))
            {
                var optoken = tokenizer.Current.Value;
                MoveNext(tokenizer);
                return new OperatorExpression(parent, expression, optoken, Parse(parent, tokenizer));
            }

            return expression;
        }

        #endregion
    }
}
