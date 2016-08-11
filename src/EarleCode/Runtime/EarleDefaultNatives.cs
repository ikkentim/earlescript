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

using System;
using System.Diagnostics;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
	internal class EarleDefaultNatives
	{
		[EarleNativeFunction]
		private static void Print(string value)
		{
			Console.WriteLine(value);
		}

		[EarleNativeFunction]
		private static void Wait(EarleStackFrame frame, float seconds)
		{
			if (seconds > 0)
				frame.ChildFrame = new WaitFrameExecutor(frame, seconds).Frame;
		}

		[EarleNativeFunction]
		private static bool IsDefined(EarleValue value)
		{
			return value.Value != null;
		}

		[EarleNativeFunction]
		private static EarleVector2 CreateVector2(float x, float y)
		{
			return new EarleVector2(x, y);
		}

		[EarleNativeFunction]
		private static EarleVector3 CreateVector3(float x, float y, float z)
		{
			return new EarleVector3(x, y, z);
		}

		[EarleNativeFunction]
		private static EarleStructure SpawnStruct()
		{
			return new EarleStructure();
		}

		private class WaitFrameExecutor : EarleBaseStackFrameExecutor
		{
			private readonly long _miliseconds;
			private readonly Stopwatch _stopwatch;

			public WaitFrameExecutor(EarleStackFrame parentFrame, float seconds) : base(EarleValue.Undefined)
			{
				if (parentFrame == null) throw new ArgumentNullException(nameof(parentFrame));
				Frame = parentFrame.SpawnChild(null, this, EarleStackFrame.SleepCallIP);

				_stopwatch = new Stopwatch();
				_stopwatch.Start();
				_miliseconds = (long) (seconds*1000);
			}

			public override EarleValue? Run()
			{
				return _stopwatch.ElapsedMilliseconds >= _miliseconds ? (EarleValue?) EarleValue.Undefined : null;
			}
		}
	}
}