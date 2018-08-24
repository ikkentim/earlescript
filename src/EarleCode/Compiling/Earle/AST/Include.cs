using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class Include : ASTNode
    {
        public Include(FilePosition filePosition, string path) : base(filePosition)
        {
            Path = path;
        }

        public string Path { get; }

        public override string ToString()
        {
            return $"#include {Path};";
        }
    }
}