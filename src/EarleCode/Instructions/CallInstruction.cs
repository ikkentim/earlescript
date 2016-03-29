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
using EarleCode.Values;

namespace EarleCode.Instructions
{
    internal class CallInstruction : IInstruction
    {
        #region Implementation of IInstruction

        public void Handle(RuntimeLoop loop)
        {
            var argumentCount = BitConverter.ToInt32(loop.PCode, loop.CIP);
            loop.CIP += 4;

            var functionReference = loop.Stack.Pop().As<EarleVariableReference>();
            var functions = loop.GetValue(functionReference)?.Value as EarleFunctionCollection;
            var function = functions?.FirstOrDefault(f => f.Parameters.Length == argumentCount);
            if (function == null)
            {
                if ((functions?.Count ?? 0) == 0)
                    throw new Exception($"unknown function {functionReference}");
                throw new Exception(
                    $"no overload of function {functionReference} found with {argumentCount} parameters.");
            }
            var args = new List<EarleValue>();
            for (var i = 0; i < argumentCount; i++)
                args.Add(loop.Stack.Pop());
            args.Reverse();

            loop.SubLoop = function.CreateLoop(loop.Runtime, args.ToArray());
        }

        #endregion
    }
}