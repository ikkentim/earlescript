using System;
namespace EarleCode.Instructions
{
    internal class CallWithoutTargetInstruction : CallInstruction
    {
        protected override bool HasTarget => false;
    }
}

