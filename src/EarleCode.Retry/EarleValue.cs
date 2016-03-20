using System;

namespace EarleCode.Retry
{
    public struct EarleValue
    {
        public EarleValue(int value) : this()
        {
            Value = value;
            Type = EarleValueType.Integer;
        }

        public EarleValue(float value) : this()
        {
            Value = value;
            Type = EarleValueType.Float;
        }

        public EarleValue(string value) : this()
        {
            Value = value;
            Type = EarleValueType.String;
        }

        public EarleValue(EarleFunction function) : this()
        {
            Value = function;
            Type = EarleValueType.Function;
        }

        public EarleValue(EarleVariableReference value) : this()
        {
            Value = value;
            Type = EarleValueType.Reference;
        }

        public EarleValue(object value, EarleValueType type) : this()
        {
            Value = value;
            Type = type;
        }
        
        public object Value { get; }

        public EarleValueType Type { get; }

        public void AssertOfType(EarleValueType type)
        {
            if (Type != type)
            {
                throw new Exception("invalid value type");
            }
        }

        #region Overrides of ValueType

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return "EarleValue container filled with " + (Value?.ToString() ?? Type.ToString());
        }

        #endregion
    }
}