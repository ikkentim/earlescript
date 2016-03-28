using System.Collections.Generic;

namespace EarleCode.Tokens
{
    public interface ITokenizer : IEnumerator<Token>
    {
        /// <summary>
        ///     Pushes the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        void Push(Token token);
    }
}