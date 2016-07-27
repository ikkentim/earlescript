using System;
namespace EarleCode.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EarleNativeFunctionAttribute : Attribute
    {
        public EarleNativeFunctionAttribute()
        {
        }

        public EarleNativeFunctionAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

