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
using System.Linq;
using EarleCode.Functions;
using EarleCode.Values;

namespace EarleCode.Blocks
{
    public abstract class Block : IBlock
    {
        private readonly List<IBlock> _blocks = new List<IBlock>();

        protected Block(IScriptScope scriptScope)
        {
            if (scriptScope == null) throw new ArgumentNullException(nameof(scriptScope));
            ScriptScope = scriptScope;
        }

        public virtual IScriptScope ScriptScope { get; }

        #region Implementation of IBlock

        public virtual IEnumerable<IBlock> Blocks => _blocks;

        public abstract InvocationResult Invoke(Runtime runtime, IEarleContext context);
        public abstract InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult);

        public virtual IVariable ResolveVariable(string variableName)
        {
            return ScriptScope.ResolveVariable(variableName);
        }

        public virtual IVariable AddVariable(string variableName)
        {
            return ScriptScope.AddVariable(variableName);
        }

        public virtual IEarleFunction ResolveFunction(EarleFunctionSignature functionSignature)
        {
            return ScriptScope.ResolveFunction(functionSignature);
        }

        public virtual void AddBlock(IBlock block)
        {
            _blocks.Add(block);
        }

        #endregion

        public InvocationResult InvokeBlocks(Runtime runtime, IEarleContext context)
        {
            for (var i = 0; i < Blocks.Count(); i++)
            {
                var block = Blocks.ElementAt(i);

                var result = block.Invoke(runtime, context);

                switch (result.State)
                {
                    case InvocationState.Incomplete:
                        return new InvocationResult(new IncompleteInvocationResult(context, result.Result) {Stage=i});
                    case InvocationState.Returned:
                        return result;
                }
            }

            return InvocationResult.Empty;
        }

        public InvocationResult ContinueBlocks(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            for (var i = incompleteInvocationResult.Stage; i < Blocks.Count(); i++)
            {
                var block = Blocks.ElementAt(i);

                var result = i == incompleteInvocationResult.Stage
                    ? block.Continue(runtime, incompleteInvocationResult.InnerResult)
                    : block.Invoke(runtime, incompleteInvocationResult.Context);

                switch (result.State)
                {
                    case InvocationState.Incomplete:
                        return
                            new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context,
                                result.Result) { Stage = i });
                    case InvocationState.Returned:
                        return result;
                }
            }

            return InvocationResult.Empty;
        }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Join("\n", Blocks);
        }

        #endregion
    }
}