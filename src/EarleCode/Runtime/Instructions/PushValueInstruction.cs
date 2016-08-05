namespace EarleCode.Runtime.Instructions
{
    internal class PushValueInstruction : Instruction
    {
        protected override void Handle()
        {
            Push(Frame.Function.File.GetValueInStore(GetInt32()));
        }
    }
}

