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

	        var assembly = GetType().GetTypeInfo().Assembly;

	        var cacheResource = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("slr.earle-cache"));

	        SLRParser parser;
	        if (cacheResource != null)
	        {
				var serializer= new CacheSerializer();

		        SLRParsingTable table;
		        using (var stream = assembly.GetManifestResourceStream(cacheResource))
			        table = serializer.DeserializeSLRParsingTable(stream);

		        parser = new SLRParser(table);
	        }
	        else
	        {
		        var grammar = new EnumGrammar<ProductionRuleEnum>(MultiCharSymbols);
		        parser = new SLRParser(grammar);
	        }

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