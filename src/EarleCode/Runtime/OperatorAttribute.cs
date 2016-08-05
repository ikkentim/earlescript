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
    [AttributeUsage(AttributeTargets.Field)]
    internal class OperatorAttribute : Attribute
    {
        public OperatorAttribute(string symbol, OperatorType type, int priority = 0)
        {
            Symbol = symbol;
            Type = type;
            Priority = priority;
        }

        public string Symbol { get; }
        public OperatorType Type { get; }
        public int Priority { get; }
    }
}