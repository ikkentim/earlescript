using System;

namespace EarleCode.Compiling.Parsing
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class RuleAttribute : Attribute
    {
        public RuleAttribute(params string[] rules)
        {
            Rules = rules;
        }
        
        public string[] Rules { get; }
    }
}