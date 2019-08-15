namespace EarleCode.Compiling.Parsing.CodeProject
{
    /// <summary>
    /// An item required for an LR1 Parser Construction
    /// </summary>
    public struct Lr1Item
    {
		public Lr0Item Lr0ItemId;
		public int LookAhead;

		public bool Equals(Lr1Item other)
		{
			return LookAhead == other.LookAhead && Lr0ItemId.Equals(other.Lr0ItemId);
		}

		public override bool Equals(object obj)
		{
			return obj is Lr1Item other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Lr0ItemId.GetHashCode() * 397) ^ LookAhead;
			}
		}
    };
}