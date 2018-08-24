using System.Globalization;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class NumberExpression : ValueExpression
    {
        public NumberExpression(FilePosition filePosition, float value) : base(filePosition)
        {
            FloatValue = value;
            IsFloat = true;
        }

        public NumberExpression(FilePosition filePosition, int value) : base(filePosition)
        {
            IntValue = value;
        }

        public bool IsFloat { get; }
        public float FloatValue { get; }
        public int IntValue { get; }
        
        public override string ToString()
        {
            return IsFloat ? FloatValue.ToString(CultureInfo.InvariantCulture) : IntValue.ToString();
        }
    }
}