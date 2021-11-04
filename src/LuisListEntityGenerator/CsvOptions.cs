using CommandLine;

namespace LuisListEntityGenerator
{
    [Verb("csv", true, HelpText = "Reads the csv file to generate the JSON list.")]
    public class CsvOptions
    {
        [Option('i', "input", Required = true, HelpText = "The file name of the CSV file.")]
        public string InputFilename { get; set; }
        
        [Option('o', "output", Required = true, HelpText = "The file name of the json output file.")]
        public string OutputFilename { get; set; }

        [Option('c', "canonicalfield", Required = true, HelpText = "The column that contains the canonical form of the entity.")]
        public string CanonicalField { get; set; }
        
        [Option('l', "columnlist", Required = true, HelpText = "The columns (comma seperated) that will be added as list to the entity.")]
        public string ColumnList { get; set; }
    }
}