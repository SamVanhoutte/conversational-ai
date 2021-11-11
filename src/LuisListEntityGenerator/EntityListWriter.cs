using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuisListEntityGenerator
{
    public class EntityListWriter
    {
        public bool WriteEntities(string outputFilename, List<Entity> entities)
        {
            var jsonString = JsonSerializer.Serialize(entities);
            File.WriteAllText(outputFilename, jsonString);
            return true;
        }
    }
}