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

using EarleCode.Blocks;

namespace EarleCode.Functions
{
    public abstract class ScopeBlock : Block
    {
        protected ScopeBlock(IScriptScope scriptScope) : base(scriptScope)
        {
        }

        protected VariablesTable Variables { get; } = new VariablesTable();

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
    }
}