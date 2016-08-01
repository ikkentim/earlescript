namespace EarleCode.Runtime.Instructions
{
    internal class UnaryOperatorInstruction : Instruction
    {
        private string _operator;
        public UnaryOperatorInstruction(string @operator)
        {
            _operator = @operator;
        }

        protected override void Handle()
        {
            Push(Frame.Frame.Runtime.Operators.RunUnaryOperator(_operator, Pop()));
        }
    }
}
