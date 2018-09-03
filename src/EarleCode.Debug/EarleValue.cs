using System;

namespace EarleCode.Debug
{
	public struct EarleValue
	{
		private int _intValue;
		private float _floatValue;
		private string _stringValue;
		private EarleFunction _functionValue;
		private EarleValueType _type;

		public static readonly EarleValue Null = new EarleValue();
        
		public EarleValue(int value)
		{
			_intValue = value;
			_floatValue = 0;
			_stringValue = null;
			_functionValue = null;
			_type = EarleValueType.NumberInt;
		}

		public EarleValue(float value)
		{
			_intValue = 0;
			_floatValue = value;
			_stringValue = null;
			_functionValue = null;
			_type = EarleValueType.NumberFloat;
		}

		public EarleValue(string value)
		{
			_intValue = 0;
			_floatValue = 0;
			_stringValue = value;
			_functionValue = null;
			_type = EarleValueType.String;
		}

		public EarleValue(EarleFunction value)
		{
			_intValue = 0;
			_floatValue = 0;
			_stringValue = null;
			_functionValue = value;
			_type = EarleValueType.FunctionPointer;
		}

		#region Overrides of ValueType

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>The fully qualified type name.</returns>
		public override string ToString()
		{
			switch (_type)
			{
				case EarleValueType.String:
					return _stringValue;
				default:
					return _type.ToString();
			}
		}

		#endregion
	}
}