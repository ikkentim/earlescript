using Earle.Tokens;

namespace Earle
{
    public class CompilerException : CodeException
    {
        public CompilerException(Token token, string error) : base(token, error)
        {
        }

        public CompilerException(string message) : base(message)
        {
        }
    }
}