using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Tiles;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Serialization.Converters
{
    sealed class StepConverter : JsonConverterBase<Step>
    {
        private static readonly Dictionary<string, Step> namedDirections =
            Directions.All.Enumerate()
                .ToDictionary(
                    d => d == Direction.Unknown
                        ? "base"
                        // ReSharper disable once PossibleNullReferenceException
                        : Enum.GetName(typeof(Direction), d).ToLowerInvariant(),
                    d => d.Step()
                );

        protected override Step ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
                return fromString(reader);

            throw new InvalidDataException($"Expected string value, encountered {reader.TokenType} when parsing step.");
        }

        private static Step fromString(JsonReader reader)
        {
            var value = reader.ReadAsString();
            var name = value.ToLowerInvariant();

            if (namedDirections.TryGetValue(name, out var step))
                return step;

            throw new InvalidDataException($"Encountered unknown step name {value}");
        }
    }
}
