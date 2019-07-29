namespace EarleCode.Compiling.Parsing.CodeProject
{
    /// <summary>
    /// Describes how to resolve ambiguities within a precedence group
    /// </summary>
    public enum Derivation
    {
        None,
        LeftMost,
        RightMost
    };
}