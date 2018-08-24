using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class VariableExpression : Expression
    {
        public VariableExpression(FilePosition filePosition, Variable variable) : base(filePosition)
        {
            Variable = variable;
        }

        public Variable Variable { get; }

        public override string ToString()
        {
            return Variable.ToString();
        }
    }
}