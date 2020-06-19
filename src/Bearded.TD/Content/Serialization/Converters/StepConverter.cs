using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Tiles;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class StepConverter : JsonConverterBase<Step>
    {
        private static readonly Dictionary<string, Step> namedDirections =
            Directions.All.Enumerate()
                // ReSharper disable once PossibleNullReferenceException
                .Select(d => (name: Enum.GetName(typeof(Direction), d).ToLowerInvariant(), step: d.Step()))
                .Append((name: "base", step: new Step()))
                .ToDictionary(d => d.name, d => d.step);

        protected override Step ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
                return fromString(reader);

            throw new InvalidDataException($"Expected string value, encountered {reader.TokenType} when parsing step.");
        }

        private static Step fromString(JsonReader reader)
        {
            var value = (string)reader.Value;
            var name = value.ToLowerInvariant();

            if (namedDirections.TryGetValue(name, out var step))
                return step;

            throw new InvalidDataException($"Encountered unknown step name {value}");
        }
    }
}
