using System;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode
{
    public class EarleFile : RuntimeScope
    {
        public string Name { get; }

        public List<EarleFunction> Functions { get; } = new List<EarleFunction>();

        public EarleFile(Runtime runtime, string name) : base(runtime)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!IsValidName(name)) throw new ArgumentException("invalid name", nameof(name));
            Name = name;
        }

        public static bool IsValidName(string input)
        {
            // todo improve
            return input.StartsWith("\\");
        }

        public void AddFunction(EarleFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            Functions.Add(function);
        }

        public EarleFunction GetFunction(string name)
        {
            return Functions.FirstOrDefault(f => f.Name == name);
        }

        #region Overrides of RuntimeScope

        public override EarleValue? GetValue(EarleVariableReference reference)
        {
            if (reference.File == Name || reference.File == null)
            {
                var function = GetFunction(reference.Name);

                if (function != null)
                {
                    return new EarleValue(function);
                }
            }

            return base.GetValue(reference);
        }

        protected override bool CanAssignReferenceAsLocal(EarleVariableReference reference)
        {
            return false;
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}