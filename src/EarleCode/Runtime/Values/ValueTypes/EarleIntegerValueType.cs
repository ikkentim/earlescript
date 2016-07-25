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

namespace EarleCode.Runtime.Values.ValueTypes
{
    public class EarleIntegerValueType : EarleValueType<int>
    {
        #region Overrides of EarleValueType<int>

        protected override object CastToOtherType(Type toType, int value)
        {
            if(toType == typeof(float))
                return (float)value;
            if(toType == typeof(bool))
               return value != 0;
            if(toType == typeof(string))
                return value.ToString();
            return null;
        }

        #endregion
    }
}