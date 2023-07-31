using System.IO;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class Biome : IConvertsTo<IBiome, Void>
{
    public string? Id { get; set; }
    public Color OverlayColor { get; set; }

    public IBiome ToGameModel(ModMetadata modMetadata, Void _)
    {
        return new Content.Models.Biome(
            ModAwareId.FromNameInMod(Id ?? throw new InvalidDataException(), modMetadata),
            OverlayColor);
    }
}
