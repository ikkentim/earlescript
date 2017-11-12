using System.Linq;

namespace EarleCode.Compiling.Lexing
{
    public struct Token<TTokenType> where TTokenType : struct
    {
        public Token(TTokenType type, string value, string file, int line, int column)
        {
            Type = type;
            Value = value;
            File = file;
            Line = line;
            Column = column;
        }

        public int Line { get; }
        public int Column { get; }
        public TTokenType Type { get; }
        public string Value { get; }
        public string File { get; }

        public bool Is(TTokenType type, params string[] values)
        {
            return Is(type) && (values == null || values.Contains(Value));
        }

        public bool Is(TTokenType type, string value)
        {
            return Type.Equals(type) && Value == value;
        }

        public bool Is(TTokenType type)
        {
            return Type.Equals(type);
        }

        #region Overrides of ValueType

        public override string ToString()
        {
            return $"{Line}:{Column}: {Type} `{Value}`";
        }

        #endregion
    }
}