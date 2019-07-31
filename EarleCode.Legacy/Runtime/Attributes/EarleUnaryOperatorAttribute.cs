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

namespace EarleCode.Runtime.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class EarleUnaryOperatorAttribute : Attribute
	{
		public EarleUnaryOperatorAttribute(OpCode @operator, Type type)
		{
			Operator = @operator;
			Types = new[] {type};
		}

		public EarleUnaryOperatorAttribute(OpCode @operator, Type[] types)
		{
			Operator = @operator;
			Types = types;
		}

		public OpCode Operator { get; }

		public Type[] Types { get; }
	}
}