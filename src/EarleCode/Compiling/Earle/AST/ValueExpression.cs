using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public abstract class ValueExpression : Expression
    {
        protected ValueExpression(FilePosition filePosition) : base(filePosition)
        {
        }
    }
}