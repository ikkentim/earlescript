using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StringExpression : ValueExpression
    {
        public StringExpression(FilePosition filePosition, string value) : base(filePosition)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }
    }
}