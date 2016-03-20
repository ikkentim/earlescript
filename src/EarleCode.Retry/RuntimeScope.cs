using System.Collections.Generic;

namespace EarleCode.Retry
{
    public class RuntimeScope
    {
        private readonly Dictionary<string, EarleValue> _locals = new Dictionary<string, EarleValue>();
        private readonly RuntimeScope _superScope;

        public RuntimeScope(RuntimeScope superScope) : this(superScope, null)
        {
        }

        public RuntimeScope(RuntimeScope superScope, IDictionary<string, EarleValue> initialLocals)
        {
            _superScope = superScope;

            if (initialLocals != null)
            {
                foreach (var i in initialLocals)
                    _locals[i.Key] = i.Value;
            }
        }

        public virtual EarleValue? GetValue(EarleVariableReference reference)
        {
            var value = _superScope?.GetValue(reference);

            if (value != null)
                return value;

            if (value == null)
            {
                EarleValue local;
                if (_locals.TryGetValue(reference.Name, out local))
                    return local;
            }

            return null;
        }
        
        protected virtual bool CanAssignReferenceAsLocal(EarleVariableReference reference)
        {
            return reference.File == null;
        }

        public virtual bool SetValue(EarleVariableReference reference, EarleValue value)
        {
            if (_superScope?.GetValue(reference) != null)
            {
                return _superScope.SetValue(reference, value);
            }

            if (CanAssignReferenceAsLocal(reference))
            {
                _locals[reference.Name] = value;
                return true;
            }

            return false;
        }
    }
}