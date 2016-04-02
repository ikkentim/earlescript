﻿// EarleCode
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
    internal class NotInstruction : IInstruction
    {
        #region Implementation of IInstruction

        public void Handle(RuntimeLoop loop)
        {
            var value = loop.Stack.Pop();

            if (value.Is<int>())
                value = value.As<int>() == 0 ? EarleValue.True : EarleValue.False;
            else if(!value.HasValue)
                    value = EarleValue.True;
            else
                value = EarleValue.False;
            
            loop.Stack.Push(value);
        }

        #endregion
    }
}