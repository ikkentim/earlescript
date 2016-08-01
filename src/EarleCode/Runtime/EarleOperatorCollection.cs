using System;
using System.Collections.Generic;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public sealed class EarleOperatorCollection
    {
        private readonly Dictionary<Tuple<string, Type, Type>, Tuple<bool,Func<EarleValue, EarleValue, EarleValue>>> _binaryOperators = new Dictionary<Tuple<string, Type, Type>, Tuple<bool,Func<EarleValue, EarleValue, EarleValue>>>();
        private readonly Dictionary<Tuple<string, Type>, Func<EarleValue, EarleValue>> _unaryOperators = new Dictionary<Tuple<string, Type>, Func<EarleValue, EarleValue>>();

        public void AddBinaryOperator(string @operator, Type[] supportedLeftTypes, Type[] supportedRightTypes, Func<EarleValue,EarleValue,EarleValue> func, EarleOperatorTypeOrder order = EarleOperatorTypeOrder.Normal)
        {
            if(@operator == null) throw new ArgumentNullException(nameof(@operator));
            if(supportedLeftTypes == null) throw new ArgumentNullException(nameof(supportedLeftTypes));
            if(supportedRightTypes == null) throw new ArgumentNullException(nameof(supportedRightTypes));
            if(func == null) throw new ArgumentNullException(nameof(func));

            foreach(var left in supportedLeftTypes)
                foreach(var right in supportedRightTypes)
                    AddBinaryOperator(@operator, left, right, func, order);
        }

        public void AddBinaryOperator(string @operator, Type[] supportedTypes, Func<EarleValue, EarleValue, EarleValue> func, EarleOperatorTypeOrder order = EarleOperatorTypeOrder.Normal)
        {
            if(@operator == null) throw new ArgumentNullException(nameof(@operator));
            if(supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));
            if(func == null) throw new ArgumentNullException(nameof(func));

            AddBinaryOperator(@operator, supportedTypes, supportedTypes, func, order);
        }

        public void AddBinaryOperator(string @operator, Type supportedLeftType, Type supportedRightType, Func<EarleValue, EarleValue, EarleValue> func, EarleOperatorTypeOrder order = EarleOperatorTypeOrder.Normal)
        {
            if(@operator == null) throw new ArgumentNullException(nameof(@operator));
            if(func == null) throw new ArgumentNullException(nameof(func));

            _binaryOperators[new Tuple<string, Type, Type>(@operator, supportedLeftType, supportedRightType)] = new Tuple<bool,Func<EarleValue,EarleValue,EarleValue>>(order == EarleOperatorTypeOrder.Swap, func);

            if(order == EarleOperatorTypeOrder.Any)
                AddBinaryOperator(@operator, supportedRightType, supportedLeftType, func);
            if(order == EarleOperatorTypeOrder.Specified)
                AddBinaryOperator(@operator, supportedRightType, supportedLeftType, func, EarleOperatorTypeOrder.Swap);
        }

        public EarleValue RunBinaryOperator(string @operator, EarleValue left, EarleValue right)
        {
            Tuple<bool,Func<EarleValue, EarleValue, EarleValue>> op;
            if(!_binaryOperators.TryGetValue(new Tuple<string, Type, Type>(@operator, left.Value?.GetType(), right.Value?.GetType()), out op) &&
              !_binaryOperators.TryGetValue(new Tuple<string, Type, Type>(@operator, null, right.Value?.GetType()), out op) &&
              !_binaryOperators.TryGetValue(new Tuple<string, Type, Type>(@operator, left.Value?.GetType(), null), out op))
                return EarleValue.Undefined;
            
            if(op.Item1)
            {
                var tmp = left;
                left = right;
                right = tmp;
            }
            return op.Item2(left, right);
        }

        public void AddUnaryOperator(string @operator, Type[] supportedTypes, Func<EarleValue, EarleValue> func)
        {
            if(@operator == null) throw new ArgumentNullException(nameof(@operator));
            if(supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));
            if(func == null) throw new ArgumentNullException(nameof(func));

            foreach(var type in supportedTypes)
                AddUnaryOperator(@operator, type, func);
        }

        public void AddUnaryOperator(string @operator, Type supportedType, Func<EarleValue, EarleValue> func)
        {
            if(@operator == null) throw new ArgumentNullException(nameof(@operator));
            if(func == null) throw new ArgumentNullException(nameof(func));

            _unaryOperators[new Tuple<string, Type>(@operator, supportedType)] = func;
        }

        public EarleValue RunUnaryOperator(string @operator, EarleValue value)
        {
            Func<EarleValue, EarleValue> func;
            if(!_unaryOperators.TryGetValue(new Tuple<string, Type>(@operator, value.Value?.GetType()), out func))
                return EarleValue.Undefined;

            return func(value);
        }
    }
}
