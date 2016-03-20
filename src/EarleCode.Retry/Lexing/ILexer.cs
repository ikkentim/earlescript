using System.Collections.Generic;

namespace EarleCode.Retry.Lexing
{
    public interface ILexer : IEnumerator<Token>
    {
        /// <summary>
        ///     Pushes the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        void Push(Token token);
    }
}