using System;
using System.IO;
using Bearded.TD.Content.Mods;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

class DependencyConverter<T> : JsonConverterBase<T>
{
    private readonly IDependencyResolver<T> dependencyResolver;

    public DependencyConverter(IDependencyResolver<T> dependencyResolver)
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