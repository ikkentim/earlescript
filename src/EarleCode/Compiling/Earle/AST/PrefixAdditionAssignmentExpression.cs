using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class PrefixAdditionAssignmentExpression : Expression
    {
        public PrefixAdditionAssignmentExpression(FilePosition filePosition, Variable variable) : base(filePosition)
        {
            Variable = variable;
        }

        public Variable Variable { get; }

        public override string ToString()
        {
            return $"++{Variable}";
        }
    }
}