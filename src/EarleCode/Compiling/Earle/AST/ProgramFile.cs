using System;
using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class ProgramFile : ASTNode
    {
        public ProgramFile(FilePosition filePosition, IReadOnlyList<Include> includes,
            IReadOnlyList<FunctionDeclaration> functionDeclarations) : base(filePosition)
        {
            Includes = includes ?? throw new ArgumentNullException(nameof(includes));
            FunctionDeclarations =
                functionDeclarations ?? throw new ArgumentNullException(nameof(functionDeclarations));
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
}