using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class ExplicitFunctionIdentifierExpression : ValueExpression
    {
        public ExplicitFunctionIdentifierExpression(FilePosition filePosition, ExplicitFunctionIdentifier value) : base(filePosition)
        {
            Value = value;
        }

        public ExplicitFunctionIdentifier Value { get; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}