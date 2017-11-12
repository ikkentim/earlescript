// Archer
// Copyright 2016 Tim Potze
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
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Operators;

namespace EarleCode.Runtime.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class EarleBinaryOperatorAttribute : Attribute {
		public EarleBinaryOperatorAttribute(OpCode @operator, Type leftType, Type rightType,
			EarleOperatorParamOrder order = EarleOperatorParamOrder.Normal) {
			Operator = @operator;
			LeftTypes = new[] { leftType };
			RightTypes = new[] { rightType };
			Order = order;
		}

		public EarleBinaryOperatorAttribute(OpCode @operator, Type type,
			EarleOperatorParamOrder order = EarleOperatorParamOrder.Normal) {
			Operator = @operator;
			LeftTypes = new[] { type };
			RightTypes = new[] { type };
			Order = order;
		}

		public EarleBinaryOperatorAttribute(OpCode @operator, Type[] leftTypes, Type[] rightTypes,
			EarleOperatorParamOrder order = EarleOperatorParamOrder.Normal)
		{
			Operator = @operator;
			LeftTypes = leftTypes;
			RightTypes = rightTypes;
			Order = order;
		}

		public EarleBinaryOperatorAttribute(OpCode @operator, Type[] types,
			EarleOperatorParamOrder order = EarleOperatorParamOrder.Normal)
		{
			Operator = @operator;
			LeftTypes = types;
			RightTypes = types;
			Order = order;
		}

		public OpCode Operator { get; }

		public Type[] LeftTypes { get; }

		public Type[] RightTypes { get; }

		public EarleOperatorParamOrder Order { get; }
	}
}