namespace CodeProject.Syntax.LALR
{
	/// <summary>
	/// An item required for an LR0 Parser Construction
	/// </summary>
	public class Lr0Item
	{
		public int Production {get;set;}
		public int Position {get; set;}
		
		public bool Equals(Lr0Item item)
		{
			return (Production == item.Production) && (Position == item.Position);
		}
	};
	
	/// <summary>
	/// An item required for an LR1 Parser Construction
	/// </summary>
	public class Lr1Item
	{
		public int Lr0ItemId {get;set;}
		public int LookAhead {get;set;}
		
		public bool Equals(Lr1Item item)
		{
			return (Lr0ItemId == item.Lr0ItemId) && (LookAhead == item.LookAhead);
		}
	};

}

