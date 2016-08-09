using System;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
    internal class UnboxFunctionReferenceInstruction : Instruction
    {
        protected override void Handle()
        {
            var value = Frame.Executor.GetValue(GetString());

            if(value.Is<EarleFunctionCollection>())
                Push(value);
            else 
                Push(EarleValue.Undefined);
        }
    }
}