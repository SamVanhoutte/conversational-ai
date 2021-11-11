using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace LuisListEntityGenerator
{
    public class CsvParser
    {
        private readonly CsvOptions _configuration;

        public CsvParser(CsvOptions configuration)
        {
            _configuration = configuration;
        }

        public List<Entity> GenerateEntityList()
        {
            var entities = new List<Entity> { };
            if (File.Exists(_configuration.InputFilename))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = _configuration.Delimiter
                };
                using var reader = new StreamReader(_configuration.InputFilename);
                using var csv = new CsvReader(reader, csvConfig);
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())  
                {
                    var record = new Entity()
                    {
                        CanonicalName = csv.GetField<string>(_configuration.CanonicalField),
                        Synonyms = new List<string>{} 
                    };
                    foreach (var fieldName in _configuration.ColumnList.Split(','))
                    {
                        record.Synonyms.Add(csv.GetField<string>(fieldName));
                    }
                    entities.Add(record);
                }

                return entities;
            }
            else
            {
                Console.WriteLine($"ERROR : {_configuration.InputFilename} not found");
                return null;
            }
        }
    }
}