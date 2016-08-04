using System;
namespace EarleCode.Runtime.Instructions
{
    internal class BinaryOperatorInstruction : Instruction
    {
        private OpCode _operator;
        public BinaryOperatorInstruction(OpCode @operator)
        {
            _operator = @operator;
        }

        protected override void Handle()
        {
            var right = Pop();
            var left = Pop();
            Push(Executor.Frame.Runtime.Operators.RunBinaryOperator(_operator, left, right));
        }
    }
}

