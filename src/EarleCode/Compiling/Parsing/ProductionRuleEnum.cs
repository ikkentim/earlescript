namespace EarleCode.Compiling.Parsing
{
    public enum ProductionRuleEnum
    {
        [Rule("(A)")]
        File,

//        [Rule("Identifier ( )")]
//        [Rule("Identifier ( IdentifierList )")]
//        Function,
//
//        [Rule("Identifier")]
//        [Rule("Identifier , IdentifierList")]
//        IdentifierList,
        [Rule("Identifier")]
        [Rule("A + Identifier")]
        [Rule("B + Identifier")]
        A,

        [Rule("A - Identifier")]
        [Rule("B - Identifier")]
        B
    }
}