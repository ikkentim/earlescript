namespace EarleCode.Compiling.Parsing.Grammars
{
    /// <summary>
    ///     Contains types of terminals.
    /// </summary>
    public enum TerminalType : byte
    {
        /// <summary>
        ///     An epsilon token.
        /// </summary>
        Epsilon,

        /// <summary>
        ///     Default flag.
        /// </summary>
        Default,

        /// <summary>
        ///     This terminal indicates the end of the file delimiter.
        /// </summary>
        EndOfFile
    }
}