using Earle.Tokens;

namespace Earle
{
    public class CompilerException : CodeException
    {
        public CompilerException(int line, int column, string error) : base(line, column, error)
        {
        }

        public CompilerException(Token token, string error) : base(token, error)
        {
        }

        public CompilerException(string message) : base(message)
        {
        }
    }
}