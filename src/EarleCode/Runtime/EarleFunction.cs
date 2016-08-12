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
using System.Collections.Generic;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleFunction
    {
        public EarleFunction(EarleFile file, string name, string[] parameters, byte[] pCode,
            Dictionary<int, int> callLines)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            File = file;
            Parameters = parameters;
            PCode = pCode;
            Name = name.ToLower();
            CallLines = callLines == null ? null : new Dictionary<int, int>(callLines);
        }

        public EarleFile File { get; }

        public byte[] PCode { get; }

        public string Name { get; }

        public string[] Parameters { get; }

        public Dictionary<int, int> CallLines { get; }

        public virtual IEarleStackFrameExecutor CreateFrameExecutor(EarleStackFrame parentFrame, int callerIp,
            EarleValue target, EarleValue[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            var locals = new EarleDictionary();

            var index = 0;

            if (Parameters == null)
            {
                throw new Exception("Parameters cannot be null in order for a EarleStackFrameExecutor to be created.");
            }
            foreach (var parameter in Parameters)
            {
                locals[parameter] = index >= arguments.Length ? EarleValue.Undefined : arguments[index];
                index++;
            }

            return new EarleStackFrameExecutor(this, parentFrame, callerIp, target, File, locals);
        }

        public EarleValue? Invoke(EarleCompletionHandler completionHandler, EarleValue target, params EarleValue[] args)
        {
            var thread = new EarleThread(completionHandler);
            var rootFrame = new EarleStackFrame(File.Runtime, null, null, EarleStackFrame.RootFrameIP, null, thread);
            var frame = CreateFrameExecutor(rootFrame, EarleStackFrame.RootCallIP, target,
                args?.ToArray() ?? new EarleValue[0]);
            thread.AttachExecutor(frame);

            return thread.Run();
        }
    }
}