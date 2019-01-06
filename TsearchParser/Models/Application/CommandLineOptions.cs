using CommandLine;

namespace TsearchParser.Models.Application
{
    public class CommandLineOptions
    {
        [Option('o', "output", Required = true, HelpText = @"Output file path, for example: ""C:\Output\out.csv""")]
        public string Output { get; set; }

        [Option('r', "delimiter", Required = false, HelpText = "Delimiter, for example: ,", Default = ";")]
        public string Delimiter { get; set; }

        [Option('c', "channel", Required = true, HelpText = "Channel")]
        public string Channel { get; set; }
    }
}
