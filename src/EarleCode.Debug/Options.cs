using CommandLine;

namespace EarleCode.Debug
{
    /// <summary>
    /// Represents the program running options
    /// </summary>
    class Options
    {
        [Option('i', "input", HelpText = "Input file to run.")]
        public string InputFile { get; set; }

        [Option('f', "function", HelpText = "The function to run in the input file.")]
        public string Function { get; set; }

        [Option("cache", HelpText = "Populate compiler cache files")]
        public bool PopulateCache { get; set; }
			
        [Option('n', "noprint", HelpText = "Do not print contents of print command")]
        public bool IgnorePrint { get; set; }
        
        [Option("parser-test", HelpText = "Run parser test script")]
        public bool ParserTest { get; set; }
    }
}