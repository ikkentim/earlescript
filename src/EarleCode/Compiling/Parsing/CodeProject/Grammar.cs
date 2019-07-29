namespace EarleCode.Compiling.Parsing.CodeProject
{
	/// <summary>
	/// All of the information required to make a Parser
	/// </summary>
	public class Grammar
	{
		public string[] Tokens{get;set;}
		
		public PrecedenceGroup[] PrecedenceGroups {get;set;}
	};
}

