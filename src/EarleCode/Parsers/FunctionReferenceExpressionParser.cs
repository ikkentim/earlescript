using EarleCode.Lexing;

namespace EarleCode.Parsers
{
    public class FunctionReferenceExpressionParser : Parser
    {
        #region Overrides of Parser

        protected override void Parse()
        {
            string path = null;

            // optional token to specify function is part of current file
            if (Lexer.Current.Is(TokenType.Token, ":"))
            {
                Lexer.SkipToken(TokenType.Token, ":");
                Lexer.SkipToken(TokenType.Token, ":");
            }
            // a specific path is supplied
            else if (Lexer.Current.Is(TokenType.Token, "\\"))
            {
                // Construct path to the function
                var identifier = false;
                path = "";
                do
                {
                    // check syntax
                    if (identifier)
                        Lexer.AssertToken(TokenType.Identifier);
                    else
                        Lexer.AssertToken(TokenType.Token, "\\");
                    identifier = !identifier;

                    path += Lexer.Current.Value;

                    Lexer.AssertMoveNext();
                } while (!Lexer.Current.Is(TokenType.Token, ":"));

                Lexer.SkipToken(TokenType.Token, ":");
                Lexer.SkipToken(TokenType.Token, ":");
            }

            Lexer.AssertToken(TokenType.Identifier);
            var name = Lexer.Current.Value;
            Lexer.AssertMoveNext();

            PushReference(path, name);
        }

        #endregion
    }
}