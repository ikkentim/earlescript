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
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Operators
{
    /// <summary>
    ///     Represents a collection of Earle operators.
    /// </summary>
    public sealed class EarleOperatorCollection
    {
        private readonly Dictionary<Tuple<OpCode, Type, Type>, Tuple<bool, Func<EarleValue, EarleValue, EarleValue>>>
            _binaryOperators =
                new Dictionary<Tuple<OpCode, Type, Type>, Tuple<bool, Func<EarleValue, EarleValue, EarleValue>>>();

        private readonly Dictionary<Tuple<OpCode, Type>, Func<EarleValue, EarleValue>> _unaryOperators =
            new Dictionary<Tuple<OpCode, Type>, Func<EarleValue, EarleValue>>();

        /// <summary>
        ///     Adds a binary operator.
        /// </summary>
        /// <param name="operator">The operator's operation code.</param>
        /// <param name="supportedLeftTypes">The supported types on the left side of the operator.</param>
        /// <param name="supportedRightTypes">The supported types on the right side of the operator.</param>
        /// <param name="func">The logic to perform this operator.</param>
        /// <param name="order">The order the left/right values may appear in.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     Thrown if <see cref="supportedLeftTypes" />,
        ///     <see cref="supportedRightTypes" /> or <see cref="func" /> is null.
        /// </exception>
        public void AddBinaryOperator(OpCode @operator, Type[] supportedLeftTypes, Type[] supportedRightTypes,
            Func<EarleValue, EarleValue, EarleValue> func,
            EarleOperatorParamOrder order = EarleOperatorParamOrder.Normal)
        {
            if (supportedLeftTypes == null) throw new ArgumentNullException(nameof(supportedLeftTypes));
            if (supportedRightTypes == null) throw new ArgumentNullException(nameof(supportedRightTypes));
            if (func == null) throw new ArgumentNullException(nameof(func));

            foreach (var left in supportedLeftTypes)
                foreach (var right in supportedRightTypes)
                    AddBinaryOperator(@operator, left, right, func, order);
        }

        /// <summary>
        ///     Adds a binary operator.
        /// </summary>
        /// <param name="operator">The operator's operation code.</param>
        /// <param name="supportedTypes">The supported types on both sides of the operator.</param>
        /// <param name="func">The logic to perform this operator.</param>
        /// <param name="order">The order the left/right values may appear in.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="supportedTypes" /> or <see cref="func" /> is null.</exception>
        public void AddBinaryOperator(OpCode @operator, Type[] supportedTypes,
            Func<EarleValue, EarleValue, EarleValue> func,
            EarleOperatorParamOrder order = EarleOperatorParamOrder.Normal)
        {
            if (supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));
            if (func == null) throw new ArgumentNullException(nameof(func));

            AddBinaryOperator(@operator, supportedTypes, supportedTypes, func, order);
        }

        /// <summary>
        ///     Adds a binary operator.
        /// </summary>
        /// <param name="operator">The operator's operation code.</param>
        /// <param name="supportedLeftType">The supported type on the left side of the operator.</param>
        /// <param name="supportedRightType">The supported type on the right side of the operator.</param>
        /// <param name="func">The logic to perform this operator.</param>
        /// <param name="order">The order the left/right values may appear in.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="func" /> is null.</exception>
        public void AddBinaryOperator(OpCode @operator, Type supportedLeftType, Type supportedRightType,
            Func<EarleValue, EarleValue, EarleValue> func,
            EarleOperatorParamOrder order = EarleOperatorParamOrder.Normal)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            _binaryOperators[new Tuple<OpCode, Type, Type>(@operator, supportedLeftType, supportedRightType)] =
                new Tuple<bool, Func<EarleValue, EarleValue, EarleValue>>(order == EarleOperatorParamOrder.Swap, func);

            if (order == EarleOperatorParamOrder.Any)
                AddBinaryOperator(@operator, supportedRightType, supportedLeftType, func);
            if (order == EarleOperatorParamOrder.Specified)
                AddBinaryOperator(@operator, supportedRightType, supportedLeftType, func, EarleOperatorParamOrder.Swap);
        }

        /// <summary>
        ///     Runs the specified binary operator.
        /// </summary>
        /// <param name="operator">The operator to run.</param>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>The result of the operation.</returns>
        public EarleValue RunBinaryOperator(OpCode @operator, EarleValue left, EarleValue right)
        {
            Tuple<bool, Func<EarleValue, EarleValue, EarleValue>> op;
            if (
                !_binaryOperators.TryGetValue(
                    new Tuple<OpCode, Type, Type>(@operator, left.Value?.GetType(), right.Value?.GetType()), out op) &&
                !_binaryOperators.TryGetValue(new Tuple<OpCode, Type, Type>(@operator, null, right.Value?.GetType()),
                    out op) &&
                !_binaryOperators.TryGetValue(new Tuple<OpCode, Type, Type>(@operator, left.Value?.GetType(), null),
                    out op))
                return EarleValue.Undefined;

            if (op.Item1)
            {
                var tmp = left;
                left = right;
                right = tmp;
            }
            return op.Item2(left, right);
        }

        /// <summary>
        ///     Adds an unary operator.
        /// </summary>
        /// <param name="operator">The operator's operation code.</param>
        /// <param name="supportedTypes">The supported types.</param>
        /// <param name="func">The logic to perform this operator.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="supportedTypes" /> or <see cref="func" /> is null.</exception>
        public void AddUnaryOperator(OpCode @operator, Type[] supportedTypes, Func<EarleValue, EarleValue> func)
        {
            if (supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));
            if (func == null) throw new ArgumentNullException(nameof(func));

            foreach (var type in supportedTypes)
                AddUnaryOperator(@operator, type, func);
        }

        /// <summary>
        ///     Adds an unary operator.
        /// </summary>
        /// <param name="operator">The operator's operation code.</param>
        /// <param name="supportedType">The supported type.</param>
        /// <param name="func">The logic to perform this operator.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="func" /> is null.</exception>
        public void AddUnaryOperator(OpCode @operator, Type supportedType, Func<EarleValue, EarleValue> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            
            _unaryOperators[new Tuple<OpCode, Type>(@operator, supportedType)] = func;
        }

        /// <summary>
        ///     Runs the specified unary operator.
        /// </summary>
        /// <param name="operator">The operator to run.</param>
        /// <param name="value">The value.</param>
        /// <returns>The result of the operation.</returns>
        public EarleValue RunUnaryOperator(OpCode @operator, EarleValue value)
        {
            Func<EarleValue, EarleValue> func;
            return _unaryOperators.TryGetValue(new Tuple<OpCode, Type>(@operator, value.Value?.GetType()), out func)
                ? func(value)
                : EarleValue.Undefined;
        }
    }
}