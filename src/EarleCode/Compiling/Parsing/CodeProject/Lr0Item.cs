namespace EarleCode.Compiling.Parsing.CodeProject
{
	/// <summary>
	/// An item required for an LR0 Parser Construction
	/// </summary>
	public struct Lr0Item
	{
		public Production Production;
		public int Position;

		public Lr0Item(int position, Production production)
		{
			Position = position;
			Production = production;
		}

		public bool Equals(Lr0Item other)
		{
			return Position == other.Position && Production.Id == other.Production.Id;
		}

		public override bool Equals(object obj)
		{
			return obj is Lr0Item other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Production != null ? Production.Id.GetHashCode() : 0) * 397) ^ Position;
			}
		}
	};
}

