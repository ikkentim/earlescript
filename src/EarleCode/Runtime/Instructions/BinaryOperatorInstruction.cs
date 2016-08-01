using System;
namespace EarleCode.Runtime.Instructions
{
    internal class BinaryOperatorInstruction : Instruction
    {
        private string _operator;
        public BinaryOperatorInstruction(string @operator)
        {
            _operator = @operator;
        }

        protected override void Handle()
        {
            var right = Pop();
            var left = Pop();
            Push(Frame.Frame.Runtime.Operators.RunBinaryOperator(_operator, left, right));
        }
    }
}

