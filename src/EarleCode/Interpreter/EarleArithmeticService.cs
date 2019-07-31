// EarleCode
// Copyright 2018 Tim Potze
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

namespace EarleCode.Interpreter
{
	public class EarleArithmeticService
	{
		public EarleValue Add(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.String || rhs.Type == EarleValueType.String)
				return new EarleValue(lhs.Convert(EarleValueType.String).StringValue + rhs.Convert(EarleValueType.String).StringValue);

			if ((lhs.Type & EarleValueType.Number) != 0 && (rhs.Type & EarleValueType.Number) != 0)
			{
				return lhs.Type == EarleValueType.NumberFloat || rhs.Type == EarleValueType.NumberFloat
					? new EarleValue(lhs.Convert(EarleValueType.NumberFloat).FloatValue + rhs.Convert(EarleValueType.NumberFloat).FloatValue)
					: new EarleValue(lhs.IntValue + rhs.IntValue);
			}
			
			if (lhs.Type == EarleValueType.Null)
				return rhs;
			if (rhs.Type == EarleValueType.Null)
				return lhs;

			return EarleValue.Null;
		}

		public EarleValue BitwiseAnd(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue & rhs.IntValue);
			throw new NotImplementedException();
		}

		public EarleValue BitwiseOr(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue | rhs.IntValue);
			throw new NotImplementedException();
		}

		public EarleValue BitwiseXor(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue ^ rhs.IntValue);
			throw new NotImplementedException();
		}

		public EarleValue Divide(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue / rhs.IntValue);
			throw new NotImplementedException();
		}
		
		public EarleValue Equal(EarleValue lhs, EarleValue rhs)
		{
			return lhs.Type == rhs.Type &&
			       lhs.IntValue == rhs.IntValue &&
			       lhs.FloatValue == rhs.FloatValue &&
			       lhs.FunctionValue == rhs.FunctionValue &&
			       lhs.StringValue == rhs.StringValue;
		}

		public EarleValue GreaterOrEqual(EarleValue lhs, EarleValue rhs)
		{
			if ((lhs.Type & EarleValueType.Number) != 0 && (rhs.Type & EarleValueType.Number) != 0)
			{
				if (lhs.Type == EarleValueType.NumberFloat || rhs.Type == EarleValueType.NumberFloat)
					return lhs.Convert(EarleValueType.NumberFloat).FloatValue >= rhs.Convert(EarleValueType.NumberFloat).FloatValue;

				return lhs.IntValue >= rhs.IntValue;
			}

			return EarleValue.False;
		}

		public EarleValue GreaterThan(EarleValue lhs, EarleValue rhs)
		{
			if ((lhs.Type & EarleValueType.Number) != 0 && (rhs.Type & EarleValueType.Number) != 0)
			{
				if (lhs.Type == EarleValueType.NumberFloat || rhs.Type == EarleValueType.NumberFloat)
					return lhs.Convert(EarleValueType.NumberFloat).FloatValue > rhs.Convert(EarleValueType.NumberFloat).FloatValue;

				return lhs.IntValue > rhs.IntValue;
			}

			return EarleValue.False;
		}

		public EarleValue LessOrEqual(EarleValue lhs, EarleValue rhs)
		{
			if ((lhs.Type & EarleValueType.Number) != 0 && (rhs.Type & EarleValueType.Number) != 0)
			{
				if (lhs.Type == EarleValueType.NumberFloat || rhs.Type == EarleValueType.NumberFloat)
					return lhs.Convert(EarleValueType.NumberFloat).FloatValue <= rhs.Convert(EarleValueType.NumberFloat).FloatValue;

				return lhs.IntValue <= rhs.IntValue;
			}

			return EarleValue.False;
		}

		public EarleValue LessThan(EarleValue lhs, EarleValue rhs)
		{
			if ((lhs.Type & EarleValueType.Number) != 0 && (rhs.Type & EarleValueType.Number) != 0)
			{
				if (lhs.Type == EarleValueType.NumberFloat || rhs.Type == EarleValueType.NumberFloat)
					return lhs.Convert(EarleValueType.NumberFloat).FloatValue < rhs.Convert(EarleValueType.NumberFloat).FloatValue;

				return lhs.IntValue < rhs.IntValue;
			}

			return EarleValue.False;
		}

		public EarleValue Modulo(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue % rhs.IntValue);
			throw new NotImplementedException();
		}

		public EarleValue Multiply(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue * rhs.IntValue);
			throw new NotImplementedException();
		}

		public EarleValue NotEqual(EarleValue lhs, EarleValue rhs)
		{
			return !IsTrue(Equal(lhs, rhs));
		}

		public EarleValue ShiftLeft(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue << rhs.IntValue);
			throw new NotImplementedException();
		}

		public EarleValue ShiftRight(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue >> rhs.IntValue);
			throw new NotImplementedException();
		}

		public EarleValue Subtract(EarleValue lhs, EarleValue rhs)
		{
			if (lhs.Type == EarleValueType.NumberInt && rhs.Type == EarleValueType.NumberInt)
				return new EarleValue(lhs.IntValue - rhs.IntValue);
			throw new NotImplementedException();
		}

		public bool IsTrue(EarleValue value)
		{
			switch (value.Type)
			{
				case EarleValueType.Null:
					return false;
				case EarleValueType.NumberInt:
					return value.IntValue != 0;
				case EarleValueType.NumberFloat:
					return value.FloatValue != 0;
				default:
					return true;
			}
		}
	}
}