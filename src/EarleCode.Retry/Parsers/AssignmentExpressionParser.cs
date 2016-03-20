using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Lexing;

namespace EarleCode.Retry.Parsers
{
    class AssignmentExpressionParser : IParser
    {
        #region Implementation of IParser

        public IEnumerable<byte> Parse(Runtime runtime, Compiler compiler, EarleFile file, ILexer lexer)
        {
            lexer.AssertToken(TokenType.Identifier);

            var name = lexer.Current.Value;
            lexer.AssertMoveNext();
            
            lexer.SkipToken(TokenType.Token, "=");
            
            // TODO: replace with expression parser
            if (lexer.Current.Type == TokenType.NumberLiteral)
            {
                float fValue;
                int iValue;

                if (float.TryParse(lexer.Current.Value, out fValue))
                {
                    yield return (byte)OpCode.PushFloat;
                    foreach (var b in BitConverter.GetBytes(fValue))
                        yield return b;
                }
                else if (int.TryParse(lexer.Current.Value, out iValue))
                {
                    yield return (byte) OpCode.PushInteger;
                    foreach (var b in BitConverter.GetBytes(iValue))
                        yield return b;
                }
                else
                {
                    throw new Exception("Unknown number type");
                }
            }
            else if (lexer.Current.Type == TokenType.StringLiteral)
            {
                yield return (byte) OpCode.PushString;
                foreach (var c in lexer.Current.Value)
                    yield return (byte) c;
            }

            lexer.AssertMoveNext();
            

            yield return (byte)OpCode.PushReference;

            foreach (var c in name)
                yield return (byte)c;
            yield return 0;

            yield return (byte) OpCode.Assign;
        }

        #endregion
    }
}
