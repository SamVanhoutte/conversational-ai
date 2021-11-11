using System;
using System.Linq;
using CommandLine;

namespace LuisListEntityGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CsvOptions>(args)
                .WithParsed<CsvOptions>(csv =>
                {
                    var parser = new CsvParser(csv);
                    var writer = new EntityListWriter();
                    var records = parser.GenerateEntityList();
                    writer.WriteEntities(csv.OutputFilename, records);
                });
        }
    }
}