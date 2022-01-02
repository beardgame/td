using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class FootprintGroup : IConvertsTo<Game.Simulation.World.FootprintGroup, Void>
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class Footprint
    {
        public List<Step>? Tiles { get; set; }
        public Angle Orientation { get; set; }
    }

    public string? Id { get; set; }
    public List<Footprint>? Footprints { get; set; }

    public Game.Simulation.World.FootprintGroup ToGameModel(ModMetadata modMetadata, Void v)
    {
        _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");
        var footprints = Footprints ?? new List<Footprint>();

        return new(
            ModAwareId.FromNameInMod(Id, modMetadata),
            footprints.Select(footprint => new Bearded.TD.Tiles.Footprint(footprint.Tiles ?? Enumerable.Empty<Step>())),
            footprints.Select(footprint => footprint.Orientation)
        );
    }
}