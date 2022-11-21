using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Bearded.TD.Game.Simulation.Weapons;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class TargetingModeConverter : JsonConverterBase<ITargetingMode>
{
    private readonly ImmutableDictionary<string, ITargetingMode> targetingModesByName;

    public TargetingModeConverter()
    {
        targetingModesByName = typeof(TargetingMode)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType.IsAssignableTo(typeof(ITargetingMode)))
            .Select(p => (ITargetingMode) p.GetValue(null))
            .ToImmutableDictionary(p => p.Name);
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
