namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Contains the action types on <see cref="SLRParsingTable"/>.
    /// </summary>
    public enum SLRActionType : byte
    {
        /// <summary>
        /// ERROR action.
        /// </summary>
        Error,

        /// <summary>
        /// SHIFT n action.
        /// </summary>
        Shift,

        /// <summary>
        /// REDUCE n action.
        /// </summary>
        Reduce
    }
}