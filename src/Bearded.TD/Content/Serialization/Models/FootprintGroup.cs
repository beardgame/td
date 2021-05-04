using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    sealed class FootprintGroup : IConvertsTo<Game.Simulation.World.FootprintGroup, Void>
    {
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        public sealed class Footprint
        {
            public List<Step> Tiles { get; set; }
            public Angle Orientation { get; set; }
        }

        public string Id { get; set; }
        public List<Footprint> Footprints { get; set; }

        public Game.Simulation.World.FootprintGroup ToGameModel(ModMetadata modMetadata, Void _)
        {
            return new(
                ModAwareId.FromNameInMod(Id, modMetadata),
                Footprints.Select(footprint => new Bearded.TD.Tiles.Footprint(footprint.Tiles)),
                Footprints.Select(footprint => footprint.Orientation)
            );
        }
    }
}
