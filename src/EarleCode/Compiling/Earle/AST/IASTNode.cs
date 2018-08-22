using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EarleCode.Compiling.Earle.AST
{
    public interface IASTNode
    {
        
    }

    public class ProgramFile : IASTNode
    {
        public ProgramFile(IReadOnlyList<Include> includes, IReadOnlyList<FunctionDeclaration> functionDeclarations)
        {
            Includes = includes ?? throw new ArgumentNullException(nameof(includes));
            FunctionDeclarations = functionDeclarations ?? throw new ArgumentNullException(nameof(functionDeclarations));
        }

        public IReadOnlyList<Include> Includes { get; }
        public IReadOnlyList<FunctionDeclaration> FunctionDeclarations { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLines(Includes);

            if (Includes != null && FunctionDeclarations != null && Includes.Count > 0 &&
                FunctionDeclarations.Count > 0)
                sb.AppendLine();
            
            sb.AppendLines(FunctionDeclarations);

            return sb.ToString();
        }
    }

    public class Include : IASTNode
    {
        public Include(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public override string ToString()
        {
            return $"#include {Path};";
        }
    }
    
    public class FunctionDeclaration : IASTNode
    {
        public FunctionDeclaration(string name, IReadOnlyList<string> parameters, IReadOnlyList<Statement> statements)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters;
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
        }

        public string Name { get; }
        public IReadOnlyList<string> Parameters { get; }
        public IReadOnlyList<Statement> Statements { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(" (");
            sb.AppendList(Parameters);
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    public class Statement : IASTNode
    {
        
    }

    public class StatementBreak : Statement
    {
        public override string ToString()
        {
            return "break;";
        }
    }
    public class StatementContinue : Statement
    {
        public override string ToString()
        {
            return "continue;";
        }
    }

    public class StatementBlock : Statement
    {
        public StatementBlock(IReadOnlyList<Statement> statements)
        {
            Statements = statements;
        }

        public IReadOnlyList<Statement> Statements { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLines(Statements);
            
            return sb.ToString();
        }
    }

    public class StatementReturn : Statement
    {
        public StatementReturn(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }
        
        public override string ToString()
        {
            if (Expression == null)
                return "return;";
            
            var sb = new StringBuilder();
            sb.Append("return ");
            sb.Append(Expression);
            sb.Append(";");

            return sb.ToString();
        }
    }

    public class StatementIf : Statement
    {
        public StatementIf(Expression condition, IReadOnlyList<Statement> statements)
        {
            Condition = condition;
            Statements = statements;
        }

        public Expression Condition { get; }
        public IReadOnlyList<Statement> Statements { get; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("if (");
            sb.Append(Condition);
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    public class StatementWhile : Statement
    {
        public StatementWhile(Expression condition, IReadOnlyList<Statement> statements)
        {
            Condition = condition;
            Statements = statements;
        }

        public Expression Condition { get; }
        public IReadOnlyList<Statement> Statements { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("while (");
            sb.Append(Condition);
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    public class StatementAssignments : Statement
    {
        public StatementAssignments(IReadOnlyList<AssignmentExpression> assignments)
        {
            Assignments = assignments;
        }

        public IReadOnlyList<AssignmentExpression> Assignments { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendList(Assignments);
            sb.Append(";");

            return sb.ToString();
        }
    }

    internal static class StringBuilderExtensions
    {
        public static void AppendLines(this StringBuilder sb, IEnumerable lines)
        {
            if (lines == null)
                return;

            foreach (var line in lines)
                sb.AppendLine(line.ToString());
        }

        public static void AppendList(this StringBuilder sb, IEnumerable values)
        {
            if (values == null)
                return;

            var first = true;
            foreach (var value in values)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");
                
                sb.Append(value);
            }
        }
    }
    public class StatementFor : Statement
    {
        public StatementFor(IReadOnlyList<AssignmentExpression> assignments, Expression condition, IReadOnlyList<AssignmentExpression> increments, IReadOnlyList<Statement> statements)
        {
            Assignments = assignments;
            Condition = condition;
            Increments = increments;
            Statements = statements;
        }

        public IReadOnlyList<AssignmentExpression> Assignments { get; }
        public Expression Condition { get; }
        public IReadOnlyList<AssignmentExpression> Increments { get; }
        public IReadOnlyList<Statement> Statements { get; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("for (");
            sb.AppendList(Assignments);
            sb.Append("; ");
            if (Condition != null)
                sb.Append(Condition);
            
            sb.Append("; ");
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.Append("} while (");
            sb.Append(Condition);
            sb.AppendLine(");");

            return sb.ToString();
        }
    }

    public class StatementDoWhile : Statement
    {
        public StatementDoWhile(Expression condition, IReadOnlyList<Statement> statements)
        {
            Condition = condition;
            Statements = statements;
        }

        public Expression Condition { get; }
        public IReadOnlyList<Statement> Statements { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("do");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.Append("} while (");
            sb.Append(Condition);
            sb.AppendLine(");");

            return sb.ToString();
        }
    }

    public class StatementFunctionCall : Statement
    {
        public StatementFunctionCall(FunctionCall function)
        {
            Function = function;
        }

        public FunctionCall Function { get; }

        public override string ToString()
        {
            return $"{Function};";
        }
    }

    public class FunctionCall : Expression
    {
        public FunctionCall(FunctionIdentifier functionIdentifier, IReadOnlyList<Expression> arguments, bool isThreaded, Expression target)
        {
            FunctionIdentifier = functionIdentifier;
            Arguments = arguments;
            IsThreaded = isThreaded;
            Target = target;
        }

        public FunctionIdentifier FunctionIdentifier { get; }
        public IReadOnlyList<Expression> Arguments { get; }
        public bool IsThreaded { get; }
        public Expression Target { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Target != null)
            {
                sb.Append(sb);
                sb.Append(" ");
            }

            if (IsThreaded)
            {
                sb.Append("thread ");
            }

            sb.Append(FunctionIdentifier);
            sb.Append("(");
            sb.AppendList(Arguments);
            sb.Append(")");

            return sb.ToString();
        }
    }

    public class ImplicitFunctionIdentifier : FunctionIdentifier
    {
        public ImplicitFunctionIdentifier(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }
    public class ExplicitFunctionIdentifier : FunctionIdentifier
    {
        public ExplicitFunctionIdentifier(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public string Path { get; }
        public string Name { get; }

        public override string ToString()
        {
            return Path == null ? $"::{Name}" : $"{Path}::{Name}";
        }
    }

    public class UnboxedFunctionIdentifier : FunctionIdentifier
    {
        public UnboxedFunctionIdentifier(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }

        public override string ToString()
        {
            return $"[[{Expression}]]";
        }
    }
    
    public abstract class FunctionIdentifier : IASTNode
    {
    }
    
    
    public abstract class Expression : IASTNode
    {
        
    }

    public class Variable : IASTNode
    {
        public Variable(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ExplicitFunctionIdentifierExpression : ValueExpression
    {
        public ExplicitFunctionIdentifierExpression(ExplicitFunctionIdentifier value)
        {
            Value = value;
        }

        public ExplicitFunctionIdentifier Value { get; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class StringExpression : ValueExpression
    {
        public StringExpression(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }
    }

    public class NumberExpression : ValueExpression
    {
        public NumberExpression(float value)
        {
            FloatValue = value;
            IsFloat = true;
        }

        public NumberExpression(int value)
        {
            IntValue = value;
        }

        public bool IsFloat { get; }
        public float FloatValue { get; }
        public int IntValue { get; }
        
        public override string ToString()
        {
            return IsFloat ? FloatValue.ToString(CultureInfo.InvariantCulture) : IntValue.ToString();
        }
    }

    public abstract class ValueExpression : Expression
    {
    }

    public class MultiplyExpression : BinaryExpression
    {
        public MultiplyExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "*";
    }

    public class DivideExpression : BinaryExpression
    {
        public DivideExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "/";
    }

    public class ModuloExpression : BinaryExpression
    {
        public ModuloExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "%";
    }

    public class AddExpression : BinaryExpression
    {
        public AddExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "+";
    }

    public class SubtractExpression : BinaryExpression
    {
        public SubtractExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "-";
    }

    public class ShiftLeftExpression : BinaryExpression
    {
        public ShiftLeftExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "<<";
    }

    public class ShiftRightExpression : BinaryExpression
    {
        public ShiftRightExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => ">>";
    }

    public class LessThanExpression : BinaryExpression
    {
        public LessThanExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "<";
    }

    public class GreaterThanExpression : BinaryExpression
    {
        public GreaterThanExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => ">";
    }

    public class LessOrEqualExpression : BinaryExpression
    {
        public LessOrEqualExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "<=";
    }

    public class GreaterOrEqualExpression : BinaryExpression
    {
        public GreaterOrEqualExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => ">=";
    }

    public class EqualExpression : BinaryExpression
    {
        public EqualExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "==";
    }

    public class NotEqualExpression : BinaryExpression
    {
        public NotEqualExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }

        protected override string OperatorSymbol => "!=";
    }

    public abstract class BinaryExpression : Expression
    {
        public BinaryExpression(Expression lhs, Expression rhs)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public Expression Lhs { get; }
        public Expression Rhs { get; }
        protected abstract string OperatorSymbol { get; }
        
        public override string ToString()
        {
            return $"{Lhs} {OperatorSymbol} {Rhs}";
        }
    }

    public class VariableExpression : Expression
    {
        public VariableExpression(Variable variable)
        {
            Variable = variable;
        }

        public Variable Variable { get; }

        public override string ToString()
        {
            return Variable.ToString();
        }
    }
    
    public class OrExpression : BinaryExpression
    {
        public OrExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "||";
    }

    public class AndExpression : BinaryExpression
    {
        public AndExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "&&";
    }

     public class BitwiseOrExpression : BinaryExpression
    {
        public BitwiseOrExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "|";
    }

    public class BitwiseXorExpression : BinaryExpression
    {
        public BitwiseXorExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "^";
    }

    public class BitwiseAndExpression : BinaryExpression
    {
        public BitwiseAndExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "&";
    }

    public class AssignmentExpression : Expression
    {
        public AssignmentExpression(Variable variable, Expression value)
        {
            Variable = variable;
            Value = value;
        }

        public Variable Variable { get; }
        public Expression Value { get; }

        public override string ToString()
        {
            return $"{Variable} = {Value}";
        }
    }
}