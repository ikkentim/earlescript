using System;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Parsing.AST
{
    public class AbstractSyntaxTreeLeafNode<TTokenType> : IAbstractSyntaxTreeLeafNode<TTokenType> where TTokenType : struct, IConvertible
    {
        public AbstractSyntaxTreeLeafNode(Token<TTokenType> token)
        {
            Token = token;
        }

        #region Implementation of IAbstractSyntaxTreeLeafNode<TTokenType>

        public Token<TTokenType> Token { get; }

        #endregion

        public override string ToString()
        {
            return Token.ToString();
        }
    }
}