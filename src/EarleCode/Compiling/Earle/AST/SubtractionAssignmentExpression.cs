using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class SubtractionAssignmentExpression : Expression
    {
        public SubtractionAssignmentExpression(FilePosition filePosition, Variable variable, Expression value) : base(filePosition)
        {
            Variable = variable;
            Value = value;
        }

        public Variable Variable { get; }
        public Expression Value { get; }

        public override string ToString()
        {
            return $"{Variable} -= {Value}";
        }
    }
}