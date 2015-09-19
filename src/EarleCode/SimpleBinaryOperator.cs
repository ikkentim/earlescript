// EarleCode
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

namespace EarleCode
{
    public class SimpleBinaryOperator : EarleBinaryOperator
    {
        private readonly Func<EarleValue?, EarleValue?, EarleValue> _compute;
        private readonly Func<EarleValue, bool> _isValue1Acceptable;
        private readonly Func<EarleValue, bool> _isValue2Acceptable;

        public SimpleBinaryOperator(Func<EarleValue?, EarleValue?, EarleValue> compute) : this(compute, null, null)
        {
        }

        public SimpleBinaryOperator(Func<EarleValue?, EarleValue?, EarleValue> compute,
            Func<EarleValue, bool> isValue1Acceptable, Func<EarleValue, bool> isValue2Acceptable)
        {
            if (compute == null) throw new ArgumentNullException(nameof(compute));
            _compute = compute;
            _isValue1Acceptable = isValue1Acceptable;
            _isValue2Acceptable = isValue2Acceptable;
        }

        #region Overrides of EarleBinaryOperator

        protected override EarleValue Compute(EarleValue? value1, EarleValue? value2)
        {
            return _compute(value1, value2);
        }

        protected override bool IsValue1Acceptable(EarleValue value)
        {
            return _isValue1Acceptable == null || _isValue1Acceptable(value);
        }

        protected override bool IsValue2Acceptable(EarleValue value)
        {
            return _isValue2Acceptable == null || _isValue2Acceptable(value);
        }

        #endregion
    }

    public class SimpleUnaryOperator : EarleUnaryOperator
    {
        private readonly Func<EarleValue, EarleValue> _compute;

        public SimpleUnaryOperator(Func<EarleValue, EarleValue> compute)
        {
            if (compute == null) throw new ArgumentNullException(nameof(compute));
            _compute = compute;
        }

        #region Overrides of EarleBinaryOperator

        protected override EarleValue Compute(EarleValue value)
        {
            return _compute(value);
        }
        

        #endregion
    }
}