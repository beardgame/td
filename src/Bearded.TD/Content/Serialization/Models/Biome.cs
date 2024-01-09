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
    public Content.Models.Material? Material { get; set; }

    public IBiome ToGameModel(ModMetadata modMetadata, Void v)
    {
        _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");
        _ = Material ?? throw new InvalidDataException($"{nameof(Material)} must be non-null");

        return new Content.Models.Biome(
            ModAwareId.FromNameInMod(Id, modMetadata),
            OverlayColor,
            Material);
    }
}
