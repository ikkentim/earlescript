using System;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Parsing.AST
{
    public interface IAbstractSyntaxTreeLeafNode<TTokenType> : IAbstractSyntaxTreeNode where TTokenType : struct, IConvertible
    {
        Token<TTokenType> Token { get; }
    }
}