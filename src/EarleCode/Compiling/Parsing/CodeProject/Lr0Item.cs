namespace EarleCode.Compiling.Parsing.CodeProject
{
	/// <summary>
	/// An item required for an LR0 Parser Construction
	/// </summary>
	public struct Lr0Item
	{
		private int _production;
		private int _position;

		public int Production
		{
			get => _production;
		}

		public int Position
		{
			get => _position;
		}

		public Lr0Item(int position, int production)
		{
			_position = position;
			_production = production;
		}

		public bool Equals(Lr0Item item)
		{
			return (_production == item._production) && (_position == item._position);
		}
	};
}

