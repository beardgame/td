using System.Collections.Generic;
using System.IO;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class Footprint : IConvertsTo<IFootprint, Void>
{
    public string? Id { get; set; }
    public List<Step>? Tiles { get; set; }

    public IFootprint ToGameModel(ModMetadata modMetadata, Void v)
    {
        _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");
        var tiles = Tiles ?? new List<Step>();

        return new Bearded.TD.Game.Simulation.World.Footprint(ModAwareId.FromNameInMod(Id, modMetadata), tiles);
    }
}
