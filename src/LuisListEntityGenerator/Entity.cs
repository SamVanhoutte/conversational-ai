using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LuisListEntityGenerator
{
    public class Entity
    {
        [JsonPropertyName("canonicalForm")]
        public string CanonicalName { get; set; }
        [JsonPropertyName("list")]
        public List<string> Synonyms { get; set; }
    }
}

