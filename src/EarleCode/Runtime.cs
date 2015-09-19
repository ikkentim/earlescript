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
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using EarleCode.Blocks;
using EarleCode.Functions;
using EarleCode.Parsers;

namespace EarleCode
{
    public interface IEarleBinaryOperator
    {
        InvocationResult Invoke(Runtime runtime, IEarleContext context, IExpression expression1, IExpression expression2);
        InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult);
    }

    public interface IEarleUnaryOperator
    {
        InvocationResult Invoke(Runtime runtime, IEarleContext context, IExpression expression);
        InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult);
    }

    public class OperatorIncompleteInvocationResult : IncompleteInvocationResult
    {
        public OperatorIncompleteInvocationResult(IEarleContext context, IncompleteInvocationResult innerResult,
            IExpression expression1, IExpression expression2)
            : base(context, innerResult)
        {
            Expression1 = expression1;
            Expression2 = expression2;
        }

        public OperatorIncompleteInvocationResult(IEarleContext context, IncompleteInvocationResult innerResult,
            EarleValue value1, IExpression expression2)
            : base(context, innerResult)
        {
            Value1 = value1;
            Expression2 = expression2;
        }

        public IExpression Expression1 { get; }
        public EarleValue Value1 { get; set; }
        public IExpression Expression2 { get; }
    }



    public abstract class EarleBinaryOperator : IEarleBinaryOperator
    {
        #region Implementation of IEarleBinaryOperator

        public InvocationResult Invoke(Runtime runtime, IEarleContext context, IExpression expression1, IExpression expression2)
        {
            var result1 = expression1.Invoke(runtime, context);
            if (result1.State == InvocationState.Incomplete)
                return
                    new InvocationResult(new OperatorIncompleteInvocationResult(context, result1.Result, expression1,
                        expression2));

            if (!IsValue1Acceptable(result1.ReturnValue))
                return new InvocationResult(InvocationState.None, Compute(null, null));

            var result2 = expression2.Invoke(runtime, context);
            if (result2.State == InvocationState.Incomplete)
                return
                    new InvocationResult(new OperatorIncompleteInvocationResult(context, result2.Result,
                        result1.ReturnValue, expression2));

            if (!IsValue2Acceptable(result2.ReturnValue))
                return new InvocationResult(InvocationState.None, Compute(result1.ReturnValue, null));

            return new InvocationResult(InvocationState.None, Compute(result1.ReturnValue, result2.ReturnValue));
        }

        protected virtual bool IsValue1Acceptable(EarleValue value)
        {
            return true;
        }

        protected virtual bool IsValue2Acceptable(EarleValue value)
        {
            return true;
        }

        protected abstract EarleValue Compute(EarleValue? value1, EarleValue? value2);
        
        public InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var incomplete = incompleteInvocationResult as OperatorIncompleteInvocationResult;

            var value1 = incomplete.Value1;

            if (incomplete.Expression1 != null)
            {
                var result1 = incomplete.Expression1.Invoke(runtime, incomplete.Context);
                if (result1.State == InvocationState.Incomplete)
                    return new InvocationResult(new IncompleteInvocationResult(incomplete.Context, result1.Result, 0, null));

                if (!IsValue1Acceptable(result1.ReturnValue))
                    return new InvocationResult(InvocationState.None, Compute(null, null));

                value1 = result1.ReturnValue;
            }

            var result2 = incomplete.Expression2.Invoke(runtime, incomplete.Context);
            if (result2.State == InvocationState.Incomplete)
                return new InvocationResult(new IncompleteInvocationResult(incomplete.Context, result2.Result, 1, null));

            if (!IsValue2Acceptable(result2.ReturnValue))
                return new InvocationResult(InvocationState.None, Compute(value1, null));

            return new InvocationResult(InvocationState.None, Compute(value1, result2.ReturnValue));
        }

        #endregion
    }

    public class SimpleBinaryOperator : EarleBinaryOperator
    {
        private Func<EarleValue?, EarleValue?, EarleValue> _compute; 
        private Func<EarleValue, bool> _isValue1Acceptable; 
        private Func<EarleValue, bool> _isValue2Acceptable;

        public SimpleBinaryOperator(Func<EarleValue?, EarleValue?, EarleValue> compute) : this(compute, null, null)
        {
        }

        public SimpleBinaryOperator(Func<EarleValue?, EarleValue?, EarleValue> compute, Func<EarleValue, bool> isValue1Acceptable, Func<EarleValue, bool> isValue2Acceptable)
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
    public class Runtime : IScriptScope
    {
        private readonly ICompiler _compiler;

        public Runtime(ICompiler compiler)
        {
            if (compiler == null) throw new ArgumentNullException(nameof(compiler));
            _compiler = compiler;


            SetOperator("+",
                new SimpleBinaryOperator((l, r) => EarleValue.Add(l ?? EarleValue.Null, r ?? EarleValue.Null)));
            SetOperator("-",
                new SimpleBinaryOperator((l, r) => EarleValue.Subtract(l ?? EarleValue.Null, r ?? EarleValue.Null)));
            SetOperator("==",
                new SimpleBinaryOperator(
                    (l, r) => ((l ?? EarleValue.Null).Value == (r ?? EarleValue.Null).Value) ? 1 : 0));
            SetOperator("!=",
                new SimpleBinaryOperator(
                    (l, r) => ((l ?? EarleValue.Null).Value != (r ?? EarleValue.Null).Value) ? 1 : 0));
            SetOperator("<",
                new SimpleBinaryOperator(
                    (l, r) => (float) (l ?? EarleValue.Null) < (float) (r ?? EarleValue.Null) ? 1 : 0));
            SetOperator("<=",
                new SimpleBinaryOperator(
                    (l, r) => (float) (l ?? EarleValue.Null) <= (float) (r ?? EarleValue.Null) ? 1 : 0));
            SetOperator(">",
                new SimpleBinaryOperator(
                    (l, r) => (float) (l ?? EarleValue.Null) > (float) (r ?? EarleValue.Null) ? 1 : 0));
            SetOperator(">=",
                new SimpleBinaryOperator(
                    (l, r) => (float) (l ?? EarleValue.Null) >= (float) (r ?? EarleValue.Null) ? 1 : 0));
        }

        public Runtime() : this(new Compiler())
        {
        }

        protected Dictionary<string, EarleFile> Files { get; } = new Dictionary<string, EarleFile>();

        #region Implementation of IScriptScope

        public virtual IVariable ResolveVariable(string variableName)
        {
            return null;
        }

        public virtual IEarleFunction ResolveFunction(EarleFunctionSignature functionSignature)
        {
            if (functionSignature.Name == null) throw new ArgumentNullException(nameof(functionSignature));

            // Expose functionality
            if (functionSignature.Path == null)
            {
                switch (functionSignature.Name)
                {
                    case "print":
                        return new PrintFunction();
                    case "println":
                        return new PrintLnFunction();
                    default:
                        return null;
                }
            }

            EarleFile earleFile;
            Files.TryGetValue(functionSignature.Path, out earleFile);
            return earleFile?.ResolveFunction(functionSignature);
        }

        public IVariable AddVariable(string variableName)
        {
            throw new NotImplementedException();
        }

        #endregion

        public virtual void LoadFile(string fileName, string script)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (script == null) throw new ArgumentNullException(nameof(script));
            if(Files.ContainsKey(fileName))
                throw new ArgumentException("file was already loaded", nameof(fileName));

            //todo : check valid file name

            var file = _compiler.Compile(this, fileName, script);
            Files.Add(fileName, file);
        }

        public virtual InvocationResult Invoke(IEarleContext context, string functionName, string fileName,
            params EarleValue[] args)
        {
            var earleFunction = ResolveFunction(new EarleFunctionSignature(functionName, fileName));

            if (earleFunction == null)
                throw new Exception("function not found");

            var result = earleFunction.Invoke(this, context, args);

            if (result.State == InvocationState.Incomplete)
            {
                _waitingCalls.Add(new WaitingCall(earleFunction, result.Result));
            }

            // todo how to use result if result is incomplete
            return result;
        }

        private readonly List<WaitingCall> _waitingCalls = new List<WaitingCall>();

        public void Continue()
        {
            // todo : lol this is stupid
            var calls = _waitingCalls.ToArray();
            _waitingCalls.Clear();
            foreach (var c in calls)
            {
                var result = c.Function.Continue(this, c.IncompleteInvocationResult);

                if (result.State == InvocationState.Incomplete)
                {
                    _waitingCalls.Add(new WaitingCall(c.Function, result.Result));
                }
            }
        }

        private readonly IEarleBinaryOperator[] _binaryOperators = new IEarleBinaryOperator[13];//todo don't hardcode

        public IEarleBinaryOperator GetOperator(string operatorToken)
        {
            return _binaryOperators[OperatorUtil.GetOperatorIdentifier(operatorToken)];
        }

        public void SetOperator(string operatorToken,
            IEarleBinaryOperator @operator)
        {
            _binaryOperators[OperatorUtil.GetOperatorIdentifier(operatorToken)]
                = @operator;
        }
    }

    public static class OperatorUtil
    {
        public static int GetOperatorIdentifier(string operatorToken)
        {
            switch (operatorToken)
            {
                case "+":
                    return 0;
                case "-":
                    return 1;
                case "^":
                    return 2;
                case "&":
                    return 3;
                case "*":
                    return 4;
                case "/":
                    return 5;
                case "==":
                    return 6;
                case "!=":
                    return 7;
                case "<=":
                    return 8;
                case ">=":
                    return 9;
                case ">":
                    return 10;
                case "<":
                    return 11;
                case "!":
                    return 12;
                default:
                    throw new NotImplementedException();
            }
        }
    }
    public struct WaitingCall
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WaitingCall(IEarleFunction function, IncompleteInvocationResult incompleteInvocationResult)
        {
            Function = function;
            IncompleteInvocationResult = incompleteInvocationResult;
        }

        public IEarleFunction Function { get; }
        public IncompleteInvocationResult IncompleteInvocationResult { get; }

    }
}