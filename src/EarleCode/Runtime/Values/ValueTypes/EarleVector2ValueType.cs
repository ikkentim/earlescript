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
    public class EarleVector2ValueType : EarleValueType<EarleVector2>
    {
        #region Overrides of EarleValueType<bool>

        protected override object CastToOtherType(Type toType, EarleVector2 value)
        {
            if(toType == typeof(EarleVector3))
                return new EarleVector3(value.X, value.Y, 0);
            if(toType == typeof(bool))
                return !(value.X.Equals(0) && value.Y.Equals(0));
            if(toType == typeof(string))
                return value.ToString();
            return null;
        }

        #endregion
    }
}