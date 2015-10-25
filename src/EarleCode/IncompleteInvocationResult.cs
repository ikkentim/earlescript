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

using EarleCode.Values;

namespace EarleCode
{
    public class IncompleteInvocationResult
    {
        private readonly string _name;

        public IncompleteInvocationResult(string name, IEarleContext context, IncompleteInvocationResult innerResult)
        {
            _name = name;
            Context = context;
            InnerResult = innerResult;
        }

        public IEarleContext Context { get; }
        public IncompleteInvocationResult InnerResult { get; }
        public int Stage { get; set;  }
        public EarleValue[] Data { get; set; }
        public VariablesTable Variables { get; set; }
        public IInvocationAwaitableEvent Event { get; set; }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _name;
        }

        #endregion
    }
}