using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class AdditionAssignmentExpression : Expression
    {
        public AdditionAssignmentExpression(FilePosition filePosition, Variable variable, Expression value) : base(filePosition)
        {
            Variable = variable;
            Value = value;
        }

        public Variable Variable { get; }
        public Expression Value { get; }

        public override string ToString()
        {
            return $"{Variable} += {Value}";
        }
    }
}