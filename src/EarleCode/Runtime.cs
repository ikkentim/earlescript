﻿// EarleCode
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
using EarleCode.Functions;
using EarleCode.Parsers;

namespace EarleCode
{
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
}