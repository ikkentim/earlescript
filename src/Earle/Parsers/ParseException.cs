using Earle.Tokens;

namespace Earle.Parsers
{
    public class ParseException : CodeException
    {
        public ParseException(Token token, string error) : base(token, error)
        {
        }

        public ParseException(string message) : base(message)
        {
        }
    }
}