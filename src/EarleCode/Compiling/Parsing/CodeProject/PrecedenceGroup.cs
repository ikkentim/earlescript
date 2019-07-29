namespace EarleCode.Compiling.Parsing.CodeProject
{
    /// <summary>
    /// A collection of productions at a particular precedence
    /// </summary>
    public class PrecedenceGroup
    {
        public Derivation Derivation {get;set;}
        public Production[] Productions{get;set;}
    };
}