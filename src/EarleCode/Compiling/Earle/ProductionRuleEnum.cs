// EarleCode
// Copyright Cont017 Tim Potze
// 
// Licensed under the Apache License, Version Cont.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-Cont.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics.CodeAnalysis;
using EarleCode.Compiling.Parsing.Grammars;

namespace EarleCode.Compiling.Earle
{
	/// <summary>
	///     Contains the Earle script grammar.
	/// </summary>
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	enum ProductionRuleEnum
	    {
		    [Rule("FileCont")]
		    File,
			
		    [Rule("FileCont FunctionDeclaration")]
		    [Rule("FileCont Include")]
		    [Rule("FunctionDeclaration")]
		    [Rule("Include")]
			FileCont,
		    
			[Rule("# `include` Path ;")]
			Include,

		    [Rule("Identifier ( IdentifierList ) Statement")]
		    FunctionDeclaration,

		    [Rule("IdentifierListCont")]
		    [Rule("")]
		    IdentifierList,

		    [Rule("IdentifierListCont, Identifier")]
		    [Rule("Identifier")]
		    IdentifierListCont,
			
		    [Rule("\\ PathCont")]
			Path,

			[Rule("PathCont \\ Identifier")]
			[Rule("Identifier")]
			PathCont,
			
		    [Rule("StatementsCont")]
		    [Rule("")]
		    Statements,

		    [Rule("StatementsCont Statement")]
		    [Rule("Statement")]
		    StatementsCont,
			
		    [Rule("{ Statements }")]
			StatementBlock,

			[Rule("FunctionCall ;")]
			StatementFunctionCall,
			
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

		    [Rule("`for` ( ForAssignment ; ForExpression ; ForAssignment ) Statement")]
			StatementFor,
			
		    [Rule("Assignments")]
			[Rule("")]
		    ForAssignment,
			
			[Rule("Expression")]
		    [Rule("")]
		    ForExpression,
			
		    [Rule("`return` ;")]
		    [Rule("`return` Expression ;")]
			StatementReturn,
			
		    [Rule("StatementBlock")]
		    [Rule("StatementFunctionCall")]
		    [Rule("StatementIf")]
		    [Rule("StatementWhile")]
		    [Rule("StatementDoWhile")]
		    [Rule("StatementBreak")]
		    [Rule("StatementContinue")]
		    [Rule("StatementWait")]
		    [Rule("StatementAssignments")]
		    [Rule("StatementFor")]
		    [Rule("StatementReturn")]
		    Statement,
			
			[Rule("Identifier")]
			[Rule("ExplicitFunctionIdentifier")]
			[Rule("UnboxedFunctionIdentifier")]
			FunctionIdentifier,
			
		    [Rule(":: Identifier")]
		    [Rule("Path :: Identifier")]
			ExplicitFunctionIdentifier,
			
		    [Rule("[[ Variable ]]")]
		    UnboxedFunctionIdentifier,
		    
		    [Rule("TargetlessFunctionCall")]
		    [Rule("TargetedFunctionCall")]
		    [Rule("TargetlessThreadedFunctionCall")]
		    [Rule("TargetedThreadedFunctionCall")]
		    FunctionCall,
			
		    [Rule("FunctionIdentifier ( ExpressionList )")]
		    TargetlessFunctionCall,
		    
		    [Rule("Variable FunctionIdentifier ( ExpressionList )")]
		    TargetedFunctionCall,
		    
		    [Rule("`thread` FunctionIdentifier ( ExpressionList )")]
		    TargetlessThreadedFunctionCall,
		    
		    [Rule("Variable `thread` FunctionIdentifier ( ExpressionList )")]
		    TargetedThreadedFunctionCall,
		    
			[Rule("ExpressionListCont")]
			[Rule("")]
			ExpressionList,

		    [Rule("ExpressionListCont, Expression")]
		    [Rule("Expression")]
			ExpressionListCont,

		    [Rule("Identifier")]
//		    [Rule("Variable . Identifier")] // TODO: Should be expression operators
//		    [Rule("Variable [ Expression ]")]
			Variable,
			
		    [Rule("Assignment")]
		    [Rule("Assignments , Assignment")]
		    Assignments,
			
		    [Rule("VariableAssignment")]
		    [Rule("PostfixAdditionAssignment")]
		    [Rule("PrefixAdditionAssignment")]
		    [Rule("PostfixSubtractionAssignment")]
		    [Rule("PrefixSubtractionAssignment")]
		    [Rule("AdditionAssignment")]
			Assignment,
		    
		    [Rule("Variable = Expression")]
		    VariableAssignment,
		    
		    [Rule("Variable += Expression")]
		    AdditionAssignment, // TODO: Other opterators
		    
		    [Rule("Variable ++")]
		    PostfixAdditionAssignment,

		    [Rule("++ Variable")]
		    PrefixAdditionAssignment,

		    [Rule("Variable --")]
		    PostfixSubtractionAssignment,

		    [Rule("-- Variable")]
		    PrefixSubtractionAssignment,

		    [Rule("Variable")]
		    [Rule("StringLiteral")]
		    [Rule("NumberLiteral")]
		    // TODO: vectors
		    Value,
		    
		    [Rule("Assignment")]
		    [Rule("Expression2")]
		    Expression, // assignment

		    [Rule("Expression2 || Expression3")]
		    [Rule("Expression3")]
		    Expression2, // logical OR

		    [Rule("Expression3 && Expression4")]
		    [Rule("Expression4")]
		    Expression3, // logical AND 
		    
		    [Rule("Expression4 | Expression5")]
		    [Rule("Expression5")]
		    Expression4, // bitwise OR

		    [Rule("Expression5 ^ Expression6")]
		    [Rule("Expression6")]
		    Expression5, // bitwise XOR

		    [Rule("Expression6 & Expression7")]
		    [Rule("Expression7")]
		    Expression6, // bitwise AND

		    [Rule("Expression7 == Expression8")]
		    [Rule("Expression7 != Expression8")]
		    [Rule("Expression8")]
		    Expression7, // equality

		    [Rule("Expression8 < Expression9")]
		    [Rule("Expression8 > Expression9")]
		    [Rule("Expression8 <= Expression9")]
		    [Rule("Expression8 >= Expression9")]
		    [Rule("Expression9")]
		    Expression8, // relational

		    [Rule("Expression9 << Expression10")]
		    [Rule("Expression9 >> Expression10")]
		    [Rule("Expression10")]
		    Expression9, // shift

		    [Rule("Expression10 + Expression11")]
		    [Rule("Expression10 - Expression11")]
		    [Rule("Expression11")]
		    Expression10, // additive

		    [Rule("Expression11 * Expression12")]
		    [Rule("Expression11 / Expression12")]
		    [Rule("Expression11 % Expression12")]
		    [Rule("Expression12")]
		    Expression11, // multiplicative

		    [Rule("ExplicitFunctionIdentifier")]
		    [Rule("(Expression)")]
		    [Rule("Value")]
		    [Rule("FunctionCall")]
		    Expression12, // Brackets and values

	    }
}