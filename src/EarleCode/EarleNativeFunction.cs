using System.Linq;
using EarleCode.Values;

namespace EarleCode
{
    public abstract class EarleNativeFunction : EarleFunction
    {
        protected EarleNativeFunction(string name, params string[] parameters) : base(null, name, parameters, null)
        {

        }

        #region Overrides of EarleFunction

        public override RuntimeLoop CreateLoop(Runtime runtime, EarleValue[] arguments)
        {
            if (Parameters.Length < arguments.Length)
                arguments =
                    arguments.Concat(Enumerable.Repeat(EarleValue.Undefined, Parameters.Length - arguments.Length)).ToArray();

            return new NativeRuntimeLoop(runtime, this, arguments);
        }

        #endregion

        protected abstract EarleValue Invoke(EarleValue[] arguments);

        private class NativeRuntimeLoop : RuntimeLoop
        {
            private readonly EarleValue[] _arguments;
            private readonly EarleNativeFunction _native;

            public NativeRuntimeLoop(Runtime runtime, EarleNativeFunction native, EarleValue[] arguments)
                : base(runtime, null, null)
            {
                _native = native;
                _arguments = arguments;
            }

            #region Overrides of RuntimeLoop

            public override EarleValue? Run()
            {
                return _native.Invoke(_arguments);
            }

            #endregion
        }
    }
}