using EarleCode.Values;

namespace EarleCode.Instructions
{
    internal class DereferenceIndexInstruction : Instruction
    {
        #region Overrides of Instruction

        protected override void Handle()
        {
            var index = Pop();
            var varray = Pop();
            var array = varray.As<EarleArray>();

            Push(array != null 
                ? new EarleBoxedValueReference(array, index).ToEarleValue() 
                : EarleValue.Undefined);
        }

        #endregion
    }
}