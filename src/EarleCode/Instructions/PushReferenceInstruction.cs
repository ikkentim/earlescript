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
using EarleCode.Values;

namespace EarleCode.Instructions
{
    internal class PushReferenceInstruction : IInstruction
    {
        #region Implementation of IInstruction

        public void Handle(RuntimeLoop loop)
        {
            var refString = "";

            while (loop.PCode[loop.CIP] != 0)
                refString += (char) loop.PCode[loop.CIP++];
            loop.CIP++;

            string file = null,
                name;

            if (refString.Contains("::"))
            {
                var spl = refString.Split(new[] {"::"}, StringSplitOptions.None);
                file = spl[0];
                name = spl[1];
            }
            else
                name = refString;

            loop.Stack.Push(new EarleVariableReference((file?.Length ?? 0) == 0 ? null : file, name).ToEarleValue());
        }

        #endregion
    }
}