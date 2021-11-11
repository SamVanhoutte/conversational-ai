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
        public static T ReadEntity<T>(this RecognizerResult result, string entityName, bool arrayEntity = false,
            T defaultValue = default)
        {
            try
            {
                var entities = result.Entities.ToObject<Dictionary<string, object>>();
                if (!entities?.ContainsKey(entityName) ?? false) return defaultValue;

                var token = entities[entityName] as JArray;
                if (arrayEntity)
                {
                    var vals = token.ToObject<List<List<T>>>();
                    if (vals != null)
                    {
                        var value = vals.FirstOrDefault().FirstOrDefault();
                        return value ?? defaultValue;
                    }
                }
                else
                {
                    var vals = token.ToObject<List<T>>();
                    if (vals != null)
                    {
                        var value = vals.FirstOrDefault();
                        return value ?? defaultValue;
                    }
                }

                return defaultValue;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return defaultValue;
            }
        }
    }
}