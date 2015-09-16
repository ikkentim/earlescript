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

        public abstract InvocationResult Invoke(IEarleContext context);

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