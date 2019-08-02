using EarleCode.Compiling.Parsing.Grammars;

namespace EarleCode.Compiling.Earle
{
    /// <summary>
    ///     Contains the Earle script grammar.
    /// </summary>
    internal enum OldProductionRuleEnum
    {
        #region File, Include, Function
        [Rule("FileCont")]
        File,

        [Rule("")]
        [Rule("Function FileCont")]
        [Rule("Include FileCont")]
        FileCont,

        [Rule("# `include` Path ;")]
        Include,

        [Rule("Identifier ( IdentifierList ) MultiLineStatement")]
        Function,

        #endregion

        #region Path, PathCont

        [Rule("Identifier PathCont")]
        [Rule("\\ Identifier PathCont")]
        Path,

        [Rule("")]
        [Rule("\\ Identifier PathCont")]
        PathCont,

        #endregion

        #region IdentifierList
        [Rule("")]
        [Rule("Identifier IdentifierListCont")]
        IdentifierList,

        [Rule("")]
        [Rule(", Identifier IdentifierListCont")]
        IdentifierListCont,

        #endregion

        #region Statements, ...

        [Rule("SingleLineStatement")]
        [Rule("MultiLineStatement")]
        SingleOrMultiLineStatement,

        [Rule("Statement")]
        SingleLineStatement,

        [Rule("{ Statements }")]
        MultiLineStatement,

        [Rule("")]
        [Rule("Statement Statements")]
        [Rule("MultiLineStatement Statements")]
        Statements,

        [Rule("StatementIf")]
        [Rule("StatementWhile")]
        [Rule("StatementDoWhile")]
        [Rule("StatementBreak")]
        [Rule("StatementContinue")]
        [Rule("StatementWait")]
        [Rule("StatementAssignments")]
        [Rule("StatementFor")]
        [Rule("StatementReturn")]
        [Rule("StatementFunctionCall")]
        Statement,

        [Rule("`if` ( Expression ) Statement")]
        StatementIf,

        [Rule("`while` ( Expression ) Statement")]
        StatementWhile,

        [Rule("`do` Statement `while` ( Expression ) ;")]
        StatementDoWhile,

        [Rule("`break` ;")]
        StatementBreak,

        [Rule("`continue` ;")]
        StatementContinue,

        [Rule("`wait` Expression ;")]
        StatementWait,

        [Rule("Assignments ;")]
        StatementAssignments,

        [Rule("`for` ( StatementForAssignments StatementForExpression StatementForIncrements SingleOrMultiLineStatement")]
        StatementFor,

        [Rule(";")]
        [Rule("Assignments ;")]
        StatementForAssignments,

        [Rule(";")]
        [Rule("Expression ;")]
        StatementForExpression,

        [Rule(")")]
        [Rule("Assignments )")]
        StatementForIncrements,

        [Rule("`return` StatementReturnCont")]
        StatementReturn,

        [Rule(";")]
        [Rule("Expression ;")]
        StatementReturnCont,

        [Rule("`placeholder` ;")]
        StatementFunctionCall,

        #endregion

        #region Assignments, Assignment

        [Rule("Assignment AssignmentsCont")]
        Assignments,

        [Rule("")]
        [Rule(", Assignment AssignmentsCont")]
        AssignmentsCont,

        [Rule("Variable AssignmentCont")]
        Assignment,

        [Rule("AssignmentOperator Expression")]
        AssignmentCont,

        #endregion

        #region Variable

        [Rule("Identifier Variable2")]
        Variable,

        [Rule("")]
        [Rule("Variable3")]
        Variable2,

        [Rule(". Identifier Variable2")]
        [Rule("[ Expression ] Variable2")]
        Variable3,

        #endregion

        #region AssignmentOperator, AssignmentModifiactionOperator, UnaryOperator

        [Rule("=", "+=", "-=", "/=", "*=", "&=", "|=", "%=", "^=")]
        AssignmentOperator,

        [Rule("++", "--")]
        AssignmentModificationOperator,

        [Rule("", "!", "-", "~", "@")]
        UnaryOperator,

        #endregion

        #region ExpressionList

        [Rule("")]
        [Rule("Expression ExpressionListCont")]
        ExpressionList,

        [Rule("")]
        [Rule(", Expression ExpressionListCont")]
        ExpressionListCont,

        #endregion

        #region Expression

        [Rule("Value")]
        [Rule("ExpressionKeyword")]
        [Rule("ExpressionCont")]
        [Rule("( Expression VectorCont )")]
//        [Rule("UnaryOperator Expression")]
        Expression,

        [Rule("Variable ExpressionCont2")]
        ExpressionCont,

        [Rule("")]
        [Rule("AssignmentCont")]
        ExpressionCont2,

        [Rule("`true`")]
        [Rule("`false`")]
        [Rule("`undefined`")]
        [Rule("[]")]
        ExpressionKeyword,

        [Rule("NumberLiteral")]
        [Rule("StringLiteral")]
        Value,

        [Rule("")]
        [Rule(", Expression VectorCont")]
        VectorCont,

        #endregion

        // (TARGET) (thread) functionName(args)
        // (TARGET) (thread) ::functionName(args)
        // (TARGET) (thread) \path\to::functionName(args)
        // (TARGET) (thread) path\to::functionName()

        //        [Rule("\\ Identifier PathCont :: Identifier ( ExpressionList )")]
        //        [Rule("Identifier ( ExpressionList )")]
        //        [Rule(":: Identifier ( ExpressionList )")]
        //        [Rule("[[ Variable ]] ( ExpressionList )")]
        //        FunctionCall,

        //[Rule("Thread Identifier FunctionCall2 ( ExpressionList )")]
//        [Rule("Thread [[ Variable ]] ( ExpressionList )")]
//        [Rule("Thread :: Identifier ( ExpressionList )")]
//        [Rule("Thread \\ Identifier PathCont :: Identifier ( ExpressionList )")]
        
        //[Rule("Identifier Thread FunctionCall3")]

        [Rule("Identifier Variable3 FunctionCall2")]
        [Rule("Identifier FunctionCall2")]
        [Rule("`thread` FunctionCall3")]
        [Rule(":: Identifier ( )")]
        [Rule("\\ Identifier PathCont :: Identifier ( )")]
        [Rule("[[ Variable ]] ( )")]
        FunctionCall,

        [Rule("`thread` FunctionCall3")]
        [Rule("FunctionCall3")]
        [Rule("( )")]
        FunctionCall2, // after initial Identifier

        [Rule("Identifier ( )")]
        [Rule(":: Identifier ( )")]
        [Rule("\\ Identifier PathCont :: Identifier ( )")]
        [Rule("[[ Variable ]] ( )")]
        FunctionCall3, // after thread keyword

        [Rule("")]
        FunctionCall4,

        [Rule("")]
        FunctionCall5,

        [Rule("")]
        FunctionCall6,


//        [Rule("")]
//        FunctionCall4,
//
//        [Rule("Identifier FunctionCall2 ( ExpressionList )")]
//        [Rule("[[ Variable ]] ( ExpressionList )")]
//        [Rule(":: Identifier ( ExpressionList )")]
//        [Rule("\\ Identifier PathCont :: Identifier ( ExpressionList )")]
//        FunctionCall3,
//
//        [Rule("PathCont ::")]
//        [Rule("")]
//        FunctionCall2,
//
//        [Rule("")]
//        [Rule("`thread`")]
//        Thread,
    }
}