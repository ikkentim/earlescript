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

using System.Collections.Generic;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleFunctionCollection : List<EarleFunction>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has
        ///     the default initial capacity.
        /// </summary>
        public EarleFunctionCollection()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that contains elements
        ///     copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection" /> is null.</exception>
        public EarleFunctionCollection(IEnumerable<EarleFunction> collection) : base(collection)
        {
        }

        public EarleFunction GetBestOverload(int argumentCount)
        {
            // TODO: Improve this logic
            return this.FirstOrDefault(f => f.Parameters != null && f.Parameters.Length == argumentCount)
                   ?? this.FirstOrDefault(f => f.Parameters == null);
        }

        public EarleValue? Invoke(EarleCompletionHandler completionHandler, EarleValue target, params EarleValue[] args)
        {
            return GetBestOverload(args.Length).Invoke(completionHandler, target, args);
        }

        public override string ToString()
        {
            return string.Format($"[[functions({Count})]]");
        }
    }
}