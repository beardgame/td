using System.IO;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.World;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class Biome : IConvertsTo<IBiome, IDependencyResolver<Content.Models.Material>>
{
    public string? Id { get; set; }
    public Color OverlayColor { get; set; }
    public string? Material { get; set; }

    public IBiome ToGameModel(ModMetadata modMetadata, IDependencyResolver<Content.Models.Material> materials)
    {
        return new Content.Models.Biome(
            ModAwareId.FromNameInMod(Id ?? throw new InvalidDataException(), modMetadata),
            OverlayColor,
            materials.Resolve(Material ?? ""));
    }
}
