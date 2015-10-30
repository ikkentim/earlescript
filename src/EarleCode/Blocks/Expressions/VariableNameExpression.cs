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
using System.Linq;
using EarleCode.Values;

namespace EarleCode.Blocks.Expressions
{
    public class VariableNameExpression : Block, IExpression
    {
        public IExpression[] Indexers { get; }
        public string Name { get; }

        public VariableNameExpression(IScriptScope scriptScope, string name, IExpression[] indexers)
            : base(scriptScope)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            Name = name;
            Indexers = indexers;
        }

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"{Name}{string.Concat(Indexers.Select(i => $"[{i}]"))}";
        }

        #endregion

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            var variable = ResolveVariable(Name);

            // TODO indexers have not yet been implemented
            //            if (_indexers != null)
            //                foreach (var indexer in _indexers.Where(indexer => indexer != null))
            //                {
            //                    variable = variable[indexer.Invoke(context).ReturnValue];
            //                    if (variable == null)
            //                        return new InvocationResult(InvocationState.None, EarleValue.Null);
            //                }

            return new InvocationResult(InvocationState.None, new EarleValue(variable));
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            // TODO indexers have not yet been implemented
            throw new NotImplementedException();
        }

        #endregion
    }
}