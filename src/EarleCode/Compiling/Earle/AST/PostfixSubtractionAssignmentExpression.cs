using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class PostfixSubtractionAssignmentExpression : Expression
    {
        public PostfixSubtractionAssignmentExpression(FilePosition filePosition, Variable variable) : base(filePosition)
        {
            Variable = variable;
        }

        public Variable Variable { get; }

        public override string ToString()
        {
            return $"{Variable}--";
        }
    }
}