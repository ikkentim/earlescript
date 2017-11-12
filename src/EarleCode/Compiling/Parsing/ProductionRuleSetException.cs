using System;

namespace EarleCode.Compiling.Parsing
{
    public class ProductionRuleSetException : Exception
    {
        public ProductionRuleSetException(string message) : base(message)
        {
        }

        public ProductionRuleSetException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}