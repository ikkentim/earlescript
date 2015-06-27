// Earle
// Copyright 2015 Tim Potze
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

namespace Earle.Variables
{
    public class ValueContainer
    {
        public ValueContainer()
        {
            Type = VarType.Null;
            Value = null;
        }

        public ValueContainer(VarType type, object value)
        {
            Type = type;
            Value = value;
        }

        public ValueContainer(string value)
        {
            Type = VarType.String;
            Value = value;
        }

        public ValueContainer(float value)
        {
            Type = VarType.Number;
            Value = value;
        }
        public virtual VarType Type { get; private set; }
        public virtual object Value { get; private set; }

        public ValueContainer GetValue()
        {
            return new ValueContainer(Type, Value);
        }


        public virtual void SetValue(VarType type, object value)
        {
            Type = type;
            Value = value;
        }

        public virtual void SetValue(ValueContainer value)
        {
            var v = value.GetValue();
            Type = v.Type;
            Value = v.Value;
        }

        public static ValueContainer Get(ValueContainer valueContainer)
        {
            if (valueContainer == null) throw new ArgumentNullException("valueContainer");
            return valueContainer.GetValue();
        }

        #region Overrides of Object

        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion
    }
}