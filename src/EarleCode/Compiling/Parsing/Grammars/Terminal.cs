using System;
using System.Globalization;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Parsing.Grammars
{
    /// <summary>
    ///     Represents a terminal.
    /// </summary>
    public struct Terminal : IFormattable
    {
        /// <summary>
        ///     Instantiates a new instance of the <see cref="Terminal" /> struct.
        /// </summary>
        /// <param name="type">The type of the terminal.</param>
        /// <param name="tokenType">The type of the token.</param>
        /// <param name="value">The token value.</param>
        public Terminal(TerminalType type, TokenType tokenType, string value)
        {
            Type = type;
            TokenType = tokenType;
            Value = value;
        }

        /// <summary>
        ///     Instantiates a new instance of the <see cref="Terminal" /> struct.
        /// </summary>
        /// <param name="tokenType">The type of the token.</param>
        /// <param name="value">The token value.</param>
        public Terminal(TokenType tokenType, string value)
            : this(TerminalType.Default, tokenType, value)
        {
        }

        /// <summary>
        ///     Instantiates a new instance of the <see cref="Terminal" /> struct.
        /// </summary>
        /// <param name="type">The type of the terminal.</param>
        public Terminal(TerminalType type)
            : this(type, default(TokenType), null)
        {
        }
        
        /// <summary>
        ///     Gets the flags for this terminal.
        /// </summary>
        public TerminalType Type { get; }
        
        /// <summary>
        ///     Gets the token type of this terminal.
        /// </summary>
        public TokenType TokenType { get; }

        /// <summary>
        ///     Gets the value of this terminal.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a token representing this terminal
        /// </summary>
        public Token Token => new Token(TokenType, Value);
        
        /// <summary>
        ///     Gets an epsilon.
        /// </summary>
        public static Terminal Epsilon { get; } = new Terminal(TerminalType.Epsilon);

        /// <summary>
        ///     Gets an end-of-file delimiter.
        /// </summary>
        public static Terminal EndOfFile { get; } = new Terminal(TerminalType.EndOfFile);

        /// <summary>
        ///     Indicates whether this value and the specified other value are equal.
        /// </summary>
        /// <param name="other">The other value.</param>
        /// <returns>
        ///     <see langword="true" /> if this value and the other value represent the same value; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool Equals(Terminal other)
        {
            return Type == other.Type && TokenType == other.TokenType && string.Equals(Value, other.Value);
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        ///    Returns a value indicating whether this value describes the other value.
        /// </summary>
        /// <param name="other">The other value.</param>
        /// <returns><see langword="true" /> if this value describes the other value; otherwise, <see langword="false" />.</returns>
        public bool Describes(Terminal other)
        {
            return Type == other.Type && (Value == null || string.Equals(Value, other.Value));
        }

        #region Implementation of IFormattable

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <param name="format">
        ///     The format to use.-or- A null reference (<see langword="Nothing" /> in Visual Basic) to use the
        ///     default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.
        /// </param>
        /// <param name="formatProvider">
        ///     The provider to use to format the value.-or- A null reference (<see langword="Nothing" />
        ///     in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.
        /// </param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (Type)
            {
                case TerminalType.Epsilon:
                    return "\x03B5";
                case TerminalType.EndOfFile:
                    return "$";
                default:
                    return Token.ToString(format ?? "B");
            }
        }

        #endregion

        #region Overrides of ValueType

        /// <summary>
        ///     Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same
        ///     value; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is Terminal other && Equals(other);
        }
        
        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ (int) TokenType;
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(null);
        }

        #endregion
    }
}