using System;
namespace EarleCode.Runtime.Instructions
{
    internal class CallWithoutTargetInstruction : CallInstruction
    {
        protected override bool HasTarget => false;
    }
}

