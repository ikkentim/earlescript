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

namespace EarleCode.Values.ValueTypes
{
    public class EarleBoolValueType : EarleValueType<bool>
    {
        #region Overrides of EarleValueType<bool>

        protected override EarleValue ParseOtherValueToType(EarleValue value)
        {
            if (value.Is<int>())
                return (value.As<int>() != 0).ToEarleValue();
            if (value.Is<float>())
                return (value.As<float>() != 0.0).ToEarleValue();

            if (value.Is(null))
                return false.ToEarleValue();

            return true.ToEarleValue();
        }

        #endregion
    }
}