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

using System.Collections.Generic;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
    internal class CallInstruction : Instruction
    {
        protected virtual bool HasTarget => true;

        #region Overrides of Instruction

        protected override void Handle()
        {
            Frame.Frame.SubFrame = CreateFrameExecutor();
        }

        #endregion

        protected virtual EarleStackFrameExecutor CreateFrameExecutor()
        {
            var argumentCount = GetInt32();
            var value = Pop().Value;
            var initialValue = value;
            var hasOverloads = false;
            EarleFunction function;

            // Unbox down to a function or function collection
            while(value is EarleVariableReference || value is EarleBoxedValueReference)
            {
                if(value is EarleVariableReference)
                    value = Frame.GetValue((EarleVariableReference)value).Value;
                else if(value is EarleBoxedValueReference)
                    value = ((EarleBoxedValueReference)value).GetField().Value;
            }

            if(value is EarleFunctionCollection)
            {
                var functions = (EarleFunctionCollection)value;
                hasOverloads = functions.Count > 0;
                function = functions.GetBestOverload(argumentCount);
            }
            else
            {
                function = value as EarleFunction;
            }

            if(function == null)
            {
                if(initialValue is EarleVariableReference)
                {
                    var functionReference = (EarleVariableReference)initialValue;

                    Frame.Frame.Runtime.HandleWarning(!hasOverloads
                        ? $"unknown function {functionReference}"
                        : $"no overload of function {functionReference} found with {argumentCount} parameters.");
                }
                else
                {
                    Frame.Frame.Runtime.HandleWarning($"{initialValue?.GetType()} cannot be invoked.");
                }

                for(var i = 0; i < argumentCount; i++)
                    Pop();

                Push(EarleValue.Undefined);
                return null;
            }

            var args = new List<EarleValue>();
            for(var i = 0; i < argumentCount; i++)
                args.Add(Pop());
            args.Reverse();

            var target = HasTarget ? Pop() : EarleValue.Undefined;

            return function.CreateFrameExecutor(Frame.Frame, target, args.ToArray());
        }
    }
}