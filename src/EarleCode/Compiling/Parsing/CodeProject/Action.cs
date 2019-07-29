namespace EarleCode.Compiling.Parsing.CodeProject
{
    /// <summary>
    /// A parse table entry
    /// </summary>
    public class Action
    {
        public ActionType ActionType {get;set;}
        public int ActionParameter {get;set;}
		
        public bool Equals(Action action)
        {
            return (ActionType == action.ActionType) && (ActionParameter == action.ActionParameter);
        }
    };
}