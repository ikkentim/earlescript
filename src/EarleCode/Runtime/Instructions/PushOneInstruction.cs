using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
    internal class PushOneInstruction : Instruction
    {
        protected override void Handle()
        {
            Push(1.ToEarleValue());
        }
    }
}