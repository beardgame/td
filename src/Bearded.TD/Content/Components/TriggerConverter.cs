using System;
using System.IO;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game.Simulation.GameObjects;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Components;

sealed class TriggerConverter : JsonConverterBase<ITrigger>
{
    protected override ITrigger ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String)
        {
            throw new InvalidDataException(
                $"Expected string value, encountered {reader.TokenType} when parsing trigger.");
        }

        return TriggerFactories.CreateForId(Convert.ToString(reader.Value) ?? throw new InvalidOperationException());
    }
}
