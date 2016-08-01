using System;
using System.Linq;
using System.Reflection;

namespace EarleCode.Runtime
{
    public sealed class EarleNativeCollection
    {
        private readonly EarleFunctionTable _natives = new EarleFunctionTable();

        public void Register(EarleFunction native)
        {
            if(native == null) throw new ArgumentNullException(nameof(native));

            _natives.Add(native);
        }

        public void RegisterInType<T>()
        {
            RegisterInType(typeof(T));
        }

        public void RegisterInType(Type type)
        {
            if(type == null) throw new ArgumentNullException(nameof(type));

            foreach(var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attribute = method.GetCustomAttributes(typeof(EarleNativeFunctionAttribute), true)?.FirstOrDefault() as EarleNativeFunctionAttribute;

                if(attribute == null)
                    continue;

                var name = attribute.Name?.ToLower() ?? method.Name.ToLower();

                Register(EarleNativeFunction.Create(name, null, method));
            }
        }

        public EarleFunctionCollection Get(string name)
        {
            return _natives.Get(name);
        }
    }
}

