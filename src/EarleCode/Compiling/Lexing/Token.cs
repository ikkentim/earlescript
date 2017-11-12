// EarleCode
// Copyright 2017 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EarleCode.Compiling.Lexing
{
    /// <summary>
    ///     Represents a single token.
    /// </summary>
    /// <typeparam name="TTokenType">The type of the token.</typeparam>
    public struct Token<TTokenType> : IFormattable where TTokenType : struct
    {
        /// <summary>
        ///     Instantiates a new instance of the <see cref="Token{TTokenType}" /> struct.
        /// </summary>
        /// <param name="flag">The flag attached to the token</param>
        /// <param name="type">The type of the token.</param>
        /// <param name="value">The token value.</param>
        /// <param name="file">The source file name the token was found in.</param>
        /// <param name="line">The line in the source file the token was found at.</param>
        /// <param name="column">The column in the source file the token was found at.</param>
        public Token(TokenFlag flag, TTokenType type, string value, string file, int line, int column)
        {
            Flag = flag;
            Type = type;
            Value = value;
            File = file;
            Line = line;
            Column = column;
        }

        /// <summary>
        ///     Instantiates a new instance of the <see cref="Token{TTokenType}" /> struct.
        /// </summary>
        /// <param name="flag">The flag attached to the token</param>
        public Token(TokenFlag flag)
            : this(flag, default(TTokenType), null, null, 0, 0)
        {
            Flag = flag;
        }

        /// <summary>
        ///     Instantiates a new instance of the <see cref="Token{TTokenType}" /> struct.
        /// </summary>
        /// <param name="type">The type of the token.</param>
        /// <param name="value">The token value.</param>
        /// <param name="file">The source file name the token was found in.</param>
        /// <param name="line">The line in the source file the token was found at.</param>
        /// <param name="column">The column in the source file the token was found at.</param>
        public Token(TTokenType type, string value, string file, int line, int column) 
            : this(TokenFlag.Default, type, value, file, line, column)
        {
        }
        
        /// <summary>
        ///     Instantiates a new instance of the <see cref="Token{TTokenType}" /> struct.
        /// </summary>
        /// <param name="type">The type of the token.</param>
        /// <param name="value">The token value.</param>
        public Token(TTokenType type, string value) 
            : this(type, value, null, 0, 0)
        {
        }

        /// <summary>
        ///     Gets the line in the source file this token was found at.
        /// </summary>
        public int Line { get; }

        /// <summary>
        ///     Gets the column in the source file this token was found at.
        /// </summary>
        public int Column { get; }

        public TokenFlag Flag { get; }
        /// <summary>
        ///     Gets the type of this token.
        /// </summary>
        public TTokenType Type { get; }

        /// <summary>
        ///     Gets a value indicating whether this token is empty.
        /// </summary>
        public bool IsEmpty => Flag == TokenFlag.Empty;

        /// <summary>
        ///     Gets the value of this token.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Gets the source file name this token was found in.
        /// </summary>
        public string File { get; }

        /// <summary>
        ///     Gets an empty token.
        /// </summary>
        public static Token<TTokenType> Empty { get; } = new Token<TTokenType>(TokenFlag.Empty);

        /// <summary>
        ///     Gets an end-of-file token.
        /// </summary>
        public static Token<TTokenType> EndOfFile { get; } = new Token<TTokenType>(TokenFlag.EndOfFile);

        /// <summary>
        ///     Indicates whether this value and the specified other value are equal.
        /// </summary>
        /// <param name="other">The other value.</param>
        /// <returns>
        ///     <see langword="true" /> if this value and the other value represent the same value; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool Equals(Token<TTokenType> other)
        {
            return other.Flag == Flag && Type.Equals(other.Type) && string.Equals(Value, other.Value);
        }

        /// <summary>
        ///    Returns a value indicating whether this value describes the other value.
        /// </summary>
        /// <param name="other">The other value.</param>
        /// <returns><see langword="true" /> if this value describes the other value; otherwise, <see langword="false" />.</returns>
        public bool Describes(Token<TTokenType> other)
        {
            switch (Flag)
            {
                case TokenFlag.Default:
                    if (other.Flag != TokenFlag.Default)
                        return false;

                    if (Value == null)
                        return Type.Equals(other.Type);

                    return Type.Equals(other.Type) && string.Equals(Value, other.Value);
                case TokenFlag.Empty:
                    if (other.Flag != TokenFlag.Empty)
                    {
                        return false;
                    }
                    return true;
                case TokenFlag.EndOfFile:
                    return other.Flag == TokenFlag.EndOfFile;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the name of the type in lower case.
        /// </summary>
        private string LowerCaseTypeName => string.Concat(Type.ToString()
            .Select(n => char.IsUpper(n) ? " " + char.ToLowerInvariant(n) : n.ToString())).Trim();

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
            switch (Flag)
            {
                    case TokenFlag.Empty:
                        return "(empty)";
                    case TokenFlag.EndOfFile:
                        return "EOF";
            }

            var sb = new StringBuilder();

            if(format == null)
                format = formatProvider?.GetFormat(GetType()) as string ?? "FP B";

            foreach (var c in format)
                switch (c)
                {
                    case 'l':
                        sb.Append(Line.ToString(formatProvider));
                        break;
                    case 'c':
                        sb.Append(Column.ToString(formatProvider));
                        break;
                    case 'p':
                    case 'P':
                        if (Line != 0 && Column != 0)
                        {
                            sb.Append(Line.ToString(formatProvider));
                            sb.Append(":");
                            sb.Append(Column.ToString(formatProvider));
                            sb.Append(":");
                        }
                        break;
                    case 'f':
                        sb.Append(File);
                        break;
                    case 'F':
                        if (!string.IsNullOrWhiteSpace(File))
                        {
                            sb.Append(File);
                            sb.Append(":");
                        }
                        break;
                    case 't':
                        sb.Append(Type);
                        break;
                    case 'T':
                        sb.Append(LowerCaseTypeName);
                        break;
                    case 'v':
                        sb.Append(Value);
                        break;
                    case 'B':
                        sb.Append(string.IsNullOrEmpty(Value) ? LowerCaseTypeName : Value);
                        break;
                    case 'b':
                        sb.Append(string.IsNullOrEmpty(Value) ? Type.ToString() : Value);
                        break;
                    default:
                        sb.Append(c);
                        break;
                }

            return sb.ToString();
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
            if (ReferenceEquals(null, obj)) return false;
            return obj is Token<TTokenType> && Equals((Token<TTokenType>) obj);
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return unchecked(IsEmpty ? 0 : (Type.GetHashCode() * 397) ^ (Value?.GetHashCode() ?? 0));
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString("FP B").Trim();
        }

        #endregion
    }
}