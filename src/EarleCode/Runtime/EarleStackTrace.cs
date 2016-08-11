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

namespace EarleCode.Runtime
{
	public class EarleStackTrace
	{
		public EarleStackTrace(EarleStackFrame[] frames)
		{
			Frames = frames;
		}

		public EarleStackFrame[] Frames { get; }

		public override string ToString()
		{
			var result = string.Empty;

			foreach (var frame in Frames)
			{
				var callerIP = frame.CallerIP;
				switch (callerIP)
				{
					case EarleStackFrame.RootCallIP:
						result += " at (ROOT CALL)";
						break;
					case EarleStackFrame.ThreadFrameIP:
						result += " at (THREAD START)";
						break;
					case EarleStackFrame.SleepCallIP:
						result += " at (SLEEPING)";
						break;
					case EarleStackFrame.RootFrameIP:
						break;
					default:
					{
						var callerFileName = frame.ParentFrame?.Function?.File?.Name ?? "???";
						var callerFunctionName = (frame.ParentFrame?.Function?.File?.Name ?? "???") + ":" +
						                         (frame.ParentFrame?.Function?.Name ?? "???");
						var callerFunctionArgs = string.Join(",", frame.ParentFrame?.Function?.Parameters ?? new string[0]);

						var line = 0;
						frame.ParentFrame?.Function?.CallLines?.TryGetValue(callerIP, out line);

						result += $" at {callerFunctionName}({callerFunctionArgs}) [0x{callerIP:X5}] in {callerFileName}:{line}";

						break;
					}
				}

				result += Environment.NewLine;
			}

			return result;
		}
	}
}