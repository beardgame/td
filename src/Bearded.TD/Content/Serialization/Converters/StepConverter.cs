using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Bearded.TD.Tiles;

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

        protected override Step ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return fromString(ref reader);
            }

            throw new JsonException($"Expected string value, encountered {reader.TokenType} when parsing step.");
        }

        private static Step fromString(ref Utf8JsonReader reader)
        {
            var value = reader.GetString() ?? "";
            var name = value.ToLowerInvariant();

            if (namedDirections.TryGetValue(name, out var step))
                return step;

            throw new JsonException($"Encountered unknown step name {value}");
        }
    }
}
