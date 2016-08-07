using System;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
    internal class UnboxFunctionReferenceInstruction : Instruction
    {
        protected override void Handle()
        {
            var value = Pop();

            if(!value.Is<EarleVariableReference>())
            {
                Push(EarleValue.Undefined);
                return;
            }

            value = Frame.Executor.GetValue(value.As<EarleVariableReference>());

            Push(value);
        }
    }
}