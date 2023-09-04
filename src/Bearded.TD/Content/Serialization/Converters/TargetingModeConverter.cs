using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Utilities;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class TargetingModeConverter : JsonConverterBase<ITargetingMode>
{
    private readonly ImmutableDictionary<string, ITargetingMode> targetingModesByName;

    public TargetingModeConverter()
    {
        targetingModesByName = typeof(TargetingMode)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.FieldType.IsAssignableTo(typeof(ITargetingMode)))
            .ToImmutableDictionary(p => p.Name.ToCamelCase(), p => (ITargetingMode) p.GetValue(null));
    }

    protected override ITargetingMode ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var o = reader.Value;
        if (o is not string s)
        {
            throw new InvalidDataException($"Found unexpected value for targeting mode: {o}");
        }

        return targetingModesByName.TryGetValue(s, out var mode)
            ? mode
            : throw new InvalidDataException($"Unknown targeting mode: {s}");
    }
}
