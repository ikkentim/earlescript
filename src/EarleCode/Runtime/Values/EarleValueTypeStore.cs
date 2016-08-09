using System;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode.Runtime.Values
{
    internal static class EarleValueTypeStore
    {
        private static Type[] _supportedTypes = {
            typeof(int),
            typeof(float),
            typeof(string),
            typeof(EarleVector2),
            typeof(EarleVector3),
            typeof(EarleFunction),
            typeof(EarleFunctionCollection),
            typeof(IEarleObject)
        };

        private static Type[] _supportedCastTypes = {
            typeof(bool)
        };

        private static Dictionary<Tuple<Type, Type>, Func<object, object>> _casters = new Dictionary<Tuple<Type, Type>, Func<object, object>>
        {
            [new Tuple<Type, Type>(typeof(int), typeof(float))] = v => (float)((int)v),
            [new Tuple<Type, Type>(typeof(float), typeof(int))] = v => (int)((float)v),
            [new Tuple<Type, Type>(null, typeof(string))] = v => v?.ToString(),
            [new Tuple<Type, Type>(typeof(EarleVector2), typeof(EarleVector3))] = v => {
                var vec2 = (EarleVector2)v;
                return new EarleVector3(vec2.X, vec2.Y, 0);
            },
            [new Tuple<Type, Type>(typeof(EarleVector3), typeof(EarleVector2))] = v => {
                var vec3 = (EarleVector3)v;
                return new EarleVector2(vec3.X, vec3.Y);
            },
            [new Tuple<Type, Type>(typeof(int), typeof(bool))] = v => (int)v != 0,
            [new Tuple<Type, Type>(typeof(bool), typeof(int))] = v => (bool)v ? 1 : 0
        };

        static EarleValueTypeStore()
        {
            foreach(var fr in _supportedTypes.Concat(_supportedCastTypes))
                foreach(var to in _supportedTypes.Concat(_supportedCastTypes))
                {
                    if(_casters.ContainsKey(new Tuple<Type, Type>(fr, to)))
                        continue;

                    var uni = _casters.Keys.FirstOrDefault(c => c.Item1 == null && c.Item2 == to);
                    if(uni != null)
                    {
                        _casters[new Tuple<Type, Type>(fr, to)] = _casters[uni];
                        continue;
                    }

                    var combo = _casters.Keys.Where(c => c.Item1 == fr).Select(c1 => {
                        var c2 = _casters.Keys.FirstOrDefault(c => c1.Item2 == c.Item1 && c.Item2 == to);
                        return c2 == null ? null : new Tuple<Tuple<Type, Type>, Tuple<Type, Type>>(c1, c2);
                    }).FirstOrDefault(v => v != null);

                    if(combo != null)
                        _casters[new Tuple<Type, Type>(fr, to)] = v => _casters[combo.Item2](_casters[combo.Item1](v));

                }
        }

        public static Func<object, object> GetCaster(Type from, Type to)
        {
            if(from == to)
                return v => v;
            
            Func<object, object> result;

            return _casters.TryGetValue(new Tuple<Type, Type>(from, to), out result)
                ? result
                : null;
                
        }

        public static bool IsSupportedType(Type type)
        {
            return _supportedTypes.Any(t => t.IsAssignableFrom(type)) || 
                                  _supportedCastTypes.Any(t => t.IsAssignableFrom(type));
        }
    }

}

