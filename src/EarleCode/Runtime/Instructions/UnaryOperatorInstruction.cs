namespace EarleCode.Runtime.Instructions
{
    internal class UnaryOperatorInstruction : Instruction
    {
        private OpCode _operator;
        public UnaryOperatorInstruction(OpCode @operator)
        {
            _operator = @operator;
        }

        protected override void Handle()
        {
            Push(Frame.Frame.Runtime.Operators.RunUnaryOperator(_operator, Pop()));
        }
    }
}
