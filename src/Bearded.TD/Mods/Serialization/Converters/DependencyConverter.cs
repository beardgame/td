using System;
using System.IO;
using Bearded.TD.Mods.Models;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Serialization.Converters
{
    class DependencyConverter<T> : JsonConverterBase<T> where T : IBlueprint
    {
        private readonly DependencyResolver<T> dependencyResolver;

        public DependencyConverter(DependencyResolver<T> dependencyResolver)
        {
            this.dependencyResolver = dependencyResolver;
        }

        protected override T ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new InvalidDataException(
                        $"Expected string value, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
            }

            return dependencyResolver.Resolve(Convert.ToString(reader.Value));
        }
    }
}
