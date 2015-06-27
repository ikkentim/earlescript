using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Earle.Blocks;
using Earle.Tokens;

namespace Earle.Parsers
{
    public class AssignmentParser : Parser<Assignment>
    {
        private readonly ExpressionParser _expressionParser = new ExpressionParser();

        #region Overrides of Parser<Assignment>

        public override string ParserRule
        {
            get { return "ASSIGNMENT"; }
        }

        public override Assignment Parse(Block parent, Tokenizer tokenizer)
        {
            if (tokenizer.Current.Type != TokenType.Identifier)
            {
                throw new Exception();
            }

            var name = tokenizer.Current.Value;

            if (!tokenizer.MoveNext())
            {
                throw new Exception();
            }

            if (tokenizer.Current.Type != TokenType.Token || tokenizer.Current.Value != "=")
            {
                throw new Exception();
            }

            if (!tokenizer.MoveNext())
            {
                throw new Exception();
            }
            
            // TODO: assignment is parent
            var result =  new Assignment(parent, name, _expressionParser.Parse(parent, tokenizer));

            if (tokenizer.Current.Type != TokenType.Token || tokenizer.Current.Value != ";")
            {
                throw new Exception();
            }

            if (!tokenizer.MoveNext())
            {
                throw new Exception();
            }

            return result;
        }

        #endregion
    }
}
