using System;
using System.Collections.Generic;

namespace EarleCode.Runtime
{
    public class EarleFunctionTable
    {
        private Dictionary<string, EarleFunctionCollection> _values = new Dictionary<string, EarleFunctionCollection>();

        public void Add(EarleFunction function)
        {
            if(function == null)
                throw new ArgumentNullException(nameof(function));

            EarleFunctionCollection collection;

            if(_values.TryGetValue(function.Name, out collection))
            {
                collection.Add(function);
            }
            else
            {
                collection = new EarleFunctionCollection();
                collection.Add(function);
                _values.Add(function.Name, collection);
            }
        }

        public EarleFunctionCollection Get(string functionName)
        {
            if(functionName == null)
                throw new ArgumentNullException(nameof(functionName));
            EarleFunctionCollection collection;

            _values.TryGetValue(functionName, out collection);
            return collection;
        }
    }
}

