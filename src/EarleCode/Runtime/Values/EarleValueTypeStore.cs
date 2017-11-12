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

using System;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode.Runtime.Values
{
    internal static class EarleValueTypeStore
    {
        private static readonly Type[] SupportedTypes =
        {
            typeof (int),
            typeof (float),
            typeof (string),
			typeof (EarleFunction),
            typeof (EarleFunctionCollection),
            typeof (IEarleObject)
        };

        private static readonly Type[] SupportedCastTypes =
        {
            typeof (bool)
        };

        private static readonly Dictionary<Tuple<Type, Type>, Func<object, object>> Casters = new Dictionary
            <Tuple<Type, Type>, Func<object, object>>
        {
            [new Tuple<Type, Type>(typeof (int), typeof (float))] = v => (float) ((int) v),
            [new Tuple<Type, Type>(typeof (float), typeof (int))] = v => (int) ((float) v),
            [new Tuple<Type, Type>(null, typeof (string))] = v => v?.ToString(),
            [new Tuple<Type, Type>(typeof (int), typeof (bool))] = v => (int) v != 0,
            [new Tuple<Type, Type>(typeof (bool), typeof (int))] = v => (bool) v ? 1 : 0
        };

        static EarleValueTypeStore()
        {
            foreach (var fr in SupportedTypes.Concat(SupportedCastTypes))
                foreach (var to in SupportedTypes.Concat(SupportedCastTypes))
                {
                    if (Casters.ContainsKey(new Tuple<Type, Type>(fr, to)))
                        continue;

                    var uni = Casters.Keys.FirstOrDefault(c => c.Item1 == null && c.Item2 == to);
                    if (uni != null)
                    {
                        Casters[new Tuple<Type, Type>(fr, to)] = Casters[uni];
                        continue;
                    }

                    var combo = Casters.Keys.Where(c => c.Item1 == fr).Select(c1 =>
                    {
                        var c2 = Casters.Keys.FirstOrDefault(c => c1.Item2 == c.Item1 && c.Item2 == to);
                        return c2 == null ? null : new Tuple<Tuple<Type, Type>, Tuple<Type, Type>>(c1, c2);
                    }).FirstOrDefault(v => v != null);

                    if (combo != null)
                        Casters[new Tuple<Type, Type>(fr, to)] = v => Casters[combo.Item2](Casters[combo.Item1](v));
                }
        }

        public static Func<object, object> GetCaster(Type from, Type to)
        {
            if (from == to)
                return v => v;

            Func<object, object> result;

            return Casters.TryGetValue(new Tuple<Type, Type>(from, to), out result)
                ? result
                : typeof(IEarleObject).IsAssignableFrom(from) &&
                  Casters.TryGetValue(new Tuple<Type, Type>(typeof(IEarleObject), to), out result)
                    ? result
                    : null;
        }

        public static bool IsSupportedType(Type type)
        {
            return SupportedTypes.Any(t => t.IsAssignableFrom(type)) ||
                   SupportedCastTypes.Any(t => t.IsAssignableFrom(type));
        }
    }
}