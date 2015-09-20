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

using System.Linq;
using EarleCode.Values;

namespace EarleCode.Blocks
{
    public abstract class ScopeBlock : Block
    {
        protected ScopeBlock(IScriptScope scriptScope) : base(scriptScope)
        {
        }

        protected VariablesTable Variables { get; set; } = new VariablesTable();

        #region Overrides of Block

        public override IVariable AddVariable(string variableName)
        {
            var variable = new Variable();
            Variables.Add(variableName, variable);
            return variable;
        }

        public override IVariable ResolveVariable(string variableName)
        {
            return base.ResolveVariable(variableName) ?? Variables.Resolve(variableName);
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
                        return new InvocationResult(new IncompleteInvocationResult(context, result.Result) {Stage = i});
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
                                result.Result) {Stage = i});
                    case InvocationState.Returned:
                        return result;
                }
            }

            return InvocationResult.Empty;
        }
    }
}