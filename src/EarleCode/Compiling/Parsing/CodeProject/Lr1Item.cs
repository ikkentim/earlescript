namespace EarleCode.Compiling.Parsing.CodeProject
{
    /// <summary>
    /// An item required for an LR1 Parser Construction
    /// </summary>
    public struct Lr1Item
    {
        private int _lr0ItemId;
        private int _lookAhead;

        public int Lr0ItemId
        {
            get => _lr0ItemId;
            set => _lr0ItemId = value;
        }

        public int LookAhead
        {
            get => _lookAhead;
            set => _lookAhead = value;
        }

        public bool Equals(Lr1Item item)
        {
            return (_lr0ItemId == item._lr0ItemId) && (_lookAhead == item._lookAhead);
        }
    };
}