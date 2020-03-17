using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    sealed class FootprintGroup : IBlueprint
    {
        public string Id { get; }

        public static FootprintGroup Single { get; } = new FootprintGroup("_single", Footprint.Single);

        public ReadOnlyCollection<Footprint> Footprints { get; }
        public ReadOnlyCollection<Angle> Orientations { get; }

        public FootprintGroup(string id, IEnumerable<Footprint> footprints, IEnumerable<Angle> orientations)
        {
            Id = id;
            Footprints = footprints.ToList().AsReadOnly();
            Orientations = orientations.ToList().AsReadOnly();

            if (Footprints.Count == 0)
                throw new ArgumentException("Footprint group must have at least one footprint.");

            if (Footprints.Count != Orientations.Count)
                throw new ArgumentException("Footprint group must have equal number footprints and orientations.");
        }

        public FootprintGroup(string name, params Footprint[] footprints)
            : this(name, footprints, footprints.Select(f => Angle.Zero))
        {
        }

        public PositionedFootprint Positioned(int index, Level level, Position2 position)
        {
            DebugAssert.Argument.Satisfies(index >= 0 && index < Footprints.Count);

            return new PositionedFootprint(level, this, index,
                Footprints[index].RootTileClosestToWorldPosition(position));
        }
    }
}
