using System.Linq;
using System.Reflection;
using System.Resources;
using EarleCode.Compiling.Earle;
using EarleCode.Compiling.Earle.AST;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing;
using EarleCode.Compiling.Parsing.Grammars;

namespace EarleCode.Compiling
{
    /// <summary>
    /// Represents the Earle compiler which can compile a string of Earle code to an abstract syntax tree (AST).
    /// </summary>
    public class EarleCompiler
    {
        /// <summary>
        /// Gets the symbols which consist of more than one character in the Earle language.
        /// </summary>
        public static readonly string[] MultiCharSymbols =
        {
            "::", "++", "--", "<<", ">>", "||", "&&",
            "-=", "+=", "~=", "!=", "@=", "*=", "/=",
            "%=", "+=", "-=", "&=", "|=", "^=", "<=",
            ">=", "==", "<<=", ">>="
        };

        /// <summary>
        /// Gets the reserved keywords of the Earle language.
        /// </summary>
        public static readonly string[] Keywords =
        {
            "include", "thread", "if", "else", "while", "do", 
            "break", "continue", "wait", "for", "return"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EarleCompiler"/> class.
        /// </summary>
        public EarleCompiler()
        {
            Lexer = new Lexer(MultiCharSymbols, Keywords);

	        var assembly = GetType().GetTypeInfo().Assembly;

	        var cacheResource = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("slr.earle-cache"));

	        IParser parser;
//	        if (cacheResource != null)
//	        {
//				var serializer= new CacheSerializer();
//
//		        SLRParsingTable table;
//		        using (var stream = assembly.GetManifestResourceStream(cacheResource))
//			        table = serializer.DeserializeSLRParsingTable(stream);
//
//		        parser = new SLRParser(table);
//	        }
//	        else
//	        {
		        var grammar = new EnumGrammar<ProductionRuleEnum>(MultiCharSymbols);
//		        parser = new ShiftReduceParser(new LALRParsingTableBuilder().Build(grammar));
                parser = new LALRParser(grammar);
//	        }

	        Parser = new EarleParser(parser);
        }
        
        /// <summary>
        /// Gets the lexer instance which is used by this compiler.
        /// </summary>
        public ILexer Lexer { get; }
        
        /// <summary>
        /// Gets the parser which is used by this compiler.
        /// </summary>
        public EarleParser Parser { get; }

        /// <summary>
        /// Compiles the specified input file into an AST.
        /// </summary>
        /// <param name="input">The input file contents.</param>
        /// <param name="file">The name of the file to be compiled.</param>
        /// <returns>The compiled abstract syntax tree.</returns>
        public ProgramFile Compile(string input, string file = null)
        {
            return Parser.Parse(Lexer.Tokenize(input, file).ToArray());
        }
    }
}