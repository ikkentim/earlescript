namespace EarleCode.Retry
{
    public struct EarleVariableReference
    {
        public EarleVariableReference(string file, string name)
        {
            File = file;
            Name = name;
        }

        public string File { get; }
        public string Name { get; }

        #region Overrides of ValueType

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return $"Reference to {File}::{Name}";
        }

        #endregion

        #region Equality members

        public bool Equals(EarleVariableReference other)
        {
            return string.Equals(File, other.File) && string.Equals(Name, other.Name);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
        /// </returns>
        /// <param name="obj">The object to compare with the current instance. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EarleVariableReference && Equals((EarleVariableReference)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((File != null ? File.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        #endregion

    }
}