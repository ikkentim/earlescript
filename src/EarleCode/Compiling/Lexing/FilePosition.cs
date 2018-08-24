using System;
using System.Globalization;
using System.Text;

namespace EarleCode.Compiling.Lexing
{
    /// <summary>
    /// Represents a position in a source file.
    /// </summary>
    public struct FilePosition : IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilePosition"/> struct.
        /// </summary>
        /// <param name="file">The source file name.</param>
        /// <param name="line">The line in the source file.</param>
        /// <param name="column">The column in the source file.</param>
        public FilePosition(string file, int line, int column)
        {
            File = file;
            Line = line;
            Column = column;
        }
        
        /// <summary>
        ///     Gets the source file name.
        /// </summary>
        public string File { get; }
        
        /// <summary>
        ///     Gets the line in the source file.
        /// </summary>
        public int Line { get; }

        /// <summary>
        ///     Gets the column in the source file.
        /// </summary>
        public int Column { get; }
                
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
            var sb = new StringBuilder();

            if(format == null)
                format = formatProvider?.GetFormat(GetType()) as string ?? "Fp";

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
                        if (Line != 0 && Column != 0)
                        {
                            sb.Append(Line.ToString(formatProvider));
                            sb.Append(":");
                            sb.Append(Column.ToString(formatProvider));
                        }
                        break;
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
            return obj is FilePosition && Equals((FilePosition) obj);
        }

        public bool Equals(FilePosition other)
        {
            return string.Equals(File, other.File) && Line == other.Line && Column == other.Column;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (File != null ? File.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Line;
                hashCode = (hashCode * 397) ^ Column;
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