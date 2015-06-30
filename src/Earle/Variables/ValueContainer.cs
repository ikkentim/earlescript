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

using System.Collections.Generic;

namespace Earle.Variables
{
    public class ObjectValue : Dictionary<string, ValueContainer>
    {
    }

    public class ValueContainer
    {
        public ValueContainer() : this(null)
        {
        }

        public ValueContainer(object value)
        {
            Value = value;
        }

        public virtual VarType Type
        {
            get
            {
                if (Value == null) return VarType.Null;
                if (Value is int) return VarType.Integer;
                if (Value is float) return VarType.Float;
                if (Value is string) return VarType.String;
                if (Value is ObjectValue) return VarType.Object;
                // VarType.Target;
                return VarType.Null;
            }
        }

        public virtual object Value { get; set; }

        public ValueContainer this[ValueContainer index]
        {
            get
            {
                var isDefaultIndex = index == null || index.Value == null;

                if (Type == VarType.Object)
                {
                    var @object = (ObjectValue) Value;

                    var key = isDefaultIndex ? "" : index.Value.ToString();
                    ValueContainer value;
                    if (@object.TryGetValue(key, out value))
                        return value;

                    return @object[key] = new ValueContainer();
                }

                if (isDefaultIndex)
                    return this;

                Value = new ObjectValue();
                return this[index];
            }
            set
            {
                var valueContainer = this[index];
                if (valueContainer != null)
                    valueContainer.Value = value == null ? null : value.Value;
            }
        }

        public ValueContainer Clone()
        {
            return new ValueContainer(Value);
        }

        #region Overrides of Object

        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

        public static explicit operator float(ValueContainer value)
        {
            return (float) value.Value;
        }

        public static explicit operator int(ValueContainer value)
        {
            return (int) value.Value;
        }

        public static explicit operator string(ValueContainer value)
        {
            return (string) value.Value;
        }
    }
}