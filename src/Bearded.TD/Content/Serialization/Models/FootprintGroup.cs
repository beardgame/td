using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class FootprintGroup : IConvertsTo<Game.Simulation.World.FootprintGroup, Void>
    {
        public sealed class Footprint
        {
            public List<Step> Tiles { get; set; }
            public Angle Orientation { get; set; }
        }

        public string Id { get; set; }
        public List<Footprint> Footprints { get; set; }

        public Game.Simulation.World.FootprintGroup ToGameModel(ModMetadata modMetadata, Void _)
        {
            return new Game.Simulation.World.FootprintGroup(ModAwareId.FromNameInMod(Id, modMetadata),
                Footprints.Select(footprint => new Bearded.TD.Tiles.Footprint(footprint.Tiles)),
                Footprints.Select(footprint => footprint.Orientation)
            );
        }
    }
}
