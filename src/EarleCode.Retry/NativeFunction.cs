using System;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode
{
    public class NativeFunction : EarleFunction
    {
        private readonly Func<EarleValue[],EarleValue> _native;

        public NativeFunction(string name, Func<EarleValue[],EarleValue> native, params string[] parameters)
            : base(null, name, parameters, null)
        {
            _native = native;
        }

        #region Overrides of EarleFunction

        public override RuntimeLoop CreateLoop(Runtime runtime, Stack<EarleValue> stack)
        {
            var pars = new List<EarleValue>();
            for (var i = 0; i < Parameters.Length; i++)
            {
                pars.Add(stack.Pop());
            }
            pars.Reverse();
            return new NativeRuntimeLoop(runtime, _native, pars.ToArray());
        }

        #endregion
        
        public class NativeRuntimeLoop : RuntimeLoop
        {
            private readonly Func<EarleValue[],EarleValue> _native;
            private readonly EarleValue[] _arguments;

            public NativeRuntimeLoop(Runtime runtime, Func<EarleValue[],EarleValue> native, EarleValue[] arguments) : base(runtime, null, null)
            {
                _native = native;
                _arguments = arguments;
            }

            #region Overrides of RuntimeLoop

            public override EarleValue? Run()
            {
                return _native(_arguments);
            }

            #endregion
        }
    }
}