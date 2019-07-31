using System;

namespace EarleCode.Debug
{
	public struct EarleValue
	{
		public int IntValue { get; }
		public float FloatValue { get; }
		public string StringValue { get; }
		public IEarleFunction FunctionValue { get; }
		public EarleValueType Type { get; }
		
		public static readonly EarleValue Null = new EarleValue();
		public static readonly EarleValue True = new EarleValue(1);
		public static readonly EarleValue False = new EarleValue(0);
        
		public EarleValue(int value)
		{
			IntValue = value;
			FloatValue = 0;
			StringValue = null;
			FunctionValue = null;
			Type = EarleValueType.NumberInt;
		}

		public EarleValue(float value)
		{
			IntValue = 0;
			FloatValue = value;
			StringValue = null;
			FunctionValue = null;
			Type = EarleValueType.NumberFloat;
		}

		public EarleValue(string value)
		{
			IntValue = 0;
			FloatValue = 0;
			StringValue = value;
			FunctionValue = null;
			Type = EarleValueType.String;
		}

		public EarleValue(IEarleFunction value)
		{
			IntValue = 0;
			FloatValue = 0;
			StringValue = null;
			FunctionValue = value;
			Type = EarleValueType.FunctionPointer;
		}

		public EarleValue Convert(EarleValueType targetType)
		{
			if (targetType == Type || Type == EarleValueType.Null)
				return this;

			switch (targetType)
			{
				case EarleValueType.String:
					switch (Type)
					{
						case EarleValueType.Array:
							return new EarleValue("array");
						case EarleValueType.NumberInt:
							return new EarleValue(IntValue.ToString());
						case EarleValueType.NumberFloat:
							return new EarleValue(FloatValue.ToString());
						case EarleValueType.FunctionPointer:
							return new EarleValue(FunctionValue.ToString());
						case EarleValueType.Struct:
						case EarleValueType.Vector2:
						case EarleValueType.Vector3:
						case EarleValueType.Vector4:
						default:
							throw new NotImplementedException();
					}
				case EarleValueType.NumberInt:
					switch (Type)
					{
							case EarleValueType.NumberFloat:
								return new EarleValue((int) FloatValue);
						default:
							throw new NotImplementedException();
					}
				case EarleValueType.NumberFloat:
					switch (Type)
					{
						case EarleValueType.NumberInt:
							return new EarleValue((float) IntValue);
						default:
							throw new NotImplementedException();
					}
				case EarleValueType.Null:
					return EarleValue.Null;
				case EarleValueType.FunctionPointer:
				case EarleValueType.Struct:
				case EarleValueType.Vector2:
				case EarleValueType.Vector3:
				case EarleValueType.Vector4:
				case EarleValueType.Array:
				default:
					throw new NotImplementedException();
			}
		}

		public static implicit operator EarleValue(bool value)
		{
			return value ? True : False;
		}

		#region Overrides of ValueType

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>The fully qualified type name.</returns>
		public override string ToString()
		{
			switch (Type)
			{
				case EarleValueType.String:
					return StringValue;
				default:
					return Type.ToString();
			}
		}

		#endregion
	}
}