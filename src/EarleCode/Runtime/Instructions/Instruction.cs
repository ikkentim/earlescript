// EarleCode
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

using System.Runtime.InteropServices;
using System.Text;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
	internal abstract class Instruction : IInstruction
	{
		protected EarleStackFrame Frame { get; private set; }

		#region Implementation of IInstruction

		public void Handle(EarleStackFrame frame)
		{
			Frame = frame;
			Handle();
		}

		#endregion

		protected abstract void Handle();

		private FastConvert GetConvertable()
		{
			var pCode = Frame.Function.PCode;
			var cip = Frame.Executor.CIP;
			FastConvert converter = new FastConvert();
			converter.Byte0 = pCode[cip];
			converter.Byte1 = pCode[cip + 1];
			converter.Byte2 = pCode[cip + 2];
			converter.Byte3 = pCode[cip + 3];

			Jump(4);

			return converter;
		}

		protected int GetInt32()
		{
			return GetConvertable().Int32;
		}

		protected float GetSingle()
		{
			return GetConvertable().Single;
		}

		protected string GetString()
		{
			var start = Frame.Executor.CIP;
			var pCode = Frame.Function.PCode;
			var length = 0;
			while (pCode[Frame.Executor.CIP] != 0)
			{
				length++;
				Frame.Executor.CIP++;
			}
			Frame.Executor.CIP++;
			return Encoding.ASCII.GetString(pCode, start, length);
		}

		protected EarleValue Pop()
		{
			return Frame.Executor.Stack.Pop();
		}

		protected T Pop<T>()
		{
			return Pop().CastTo<T>();
		}

		protected void Jump(int count)
		{
			Frame.Executor.CIP += count;
		}

		protected EarleValue Peek()
		{
			return Frame.Executor.Stack.Peek();
		}

		protected void Push(EarleValue item)
		{
			Frame.Executor.Stack.Push(item);
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct FastConvert
		{
			[FieldOffset(0)]
			public byte Byte0;

			[FieldOffset(1)]
			public byte Byte1;

			[FieldOffset(2)]
			public byte Byte2;

			[FieldOffset(3)]
			public byte Byte3;

			[FieldOffset(0)]
			public readonly int Int32;

			[FieldOffset(0)]
			public readonly float Single;
		}
	}
}