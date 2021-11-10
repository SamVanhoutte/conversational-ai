using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json.Linq;

namespace NasdaqBot.Extensions
{
    public static class EntitiesExtensions
    {
        public static T ReadEntity<T>(this RecognizerResult result, string entityName,
            T defaultValue = default) where T: class
        {
            try
            {
                var entities = result.Entities.ToObject<Dictionary<string, object>>();
                if (!entities?.ContainsKey(entityName)??false) return defaultValue;

                var token = entities[entityName] as JArray;
                var vals = token.ToObject<List<List<string>>>();
                var value= vals.FirstOrDefault()?.FirstOrDefault();
                if (value == null) return defaultValue;
                return value as T;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return defaultValue;
            }
        }
    }
}