using System;
using System.Linq;
using EarleCode.Compiling.Earle;
using EarleCode.Compiling.Earle.AST;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.OldCompiler.ParseCodeGen;
using IParser = EarleCode.Compiling.Parsing.IParser;

namespace EarleCode.Compiling
{
    public class EarleCompiler
    {
        public static readonly string[] MultiCharSymbols =
        {
            "::", "++", "--", "<<", ">>", "||", "&&",
            "-=", "+=", "~=", "!=", "@=", "*=", "/=",
            "%=", "+=", "-=", "&=", "|=", "^=", "<=",
            ">=", "=="
        };

        public static readonly string[] Keywords =
        {
            "include", "thread", "if", "while", "do", 
            "break", "continue", "wait", "for", "return"
        };

        public EarleCompiler()
        {
            Lexer = new Lexer(MultiCharSymbols, Keywords);
            var grammar = new EnumGrammar<ProductionRuleEnum>(MultiCharSymbols);
            var parser = new SLRParser(grammar);
            Parser = new EarleParser(parser);
        }
        
        public ILexer Lexer { get; }
        public EarleParser Parser { get; }

        public ProgramFile Compile(string input, string file = null)
        {
            return Parser.Parse(Lexer.Tokenize(input, file).ToArray());
        }
    }
}