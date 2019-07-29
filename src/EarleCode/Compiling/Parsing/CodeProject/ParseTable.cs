namespace EarleCode.Compiling.Parsing.CodeProject
{
	/// <summary>
	/// Directs the parser on which action to perform at a given state on a particular input
	/// </summary>
	public class ParseTable
	{
		public Action [,] Actions {get;set;}
	};
}

