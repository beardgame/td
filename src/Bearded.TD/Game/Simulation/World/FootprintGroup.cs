using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.World
{
    sealed class FootprintGroup : IBlueprint
    {
        public ModAwareId Id { get; }

        public static FootprintGroup Single { get; } =
            new(ModAwareId.Invalid, ImmutableArray.Create(Footprint.Single), ImmutableArray.Create(Angle.Zero));

        public ImmutableArray<Footprint> Footprints { get; }
        public ImmutableArray<Angle> Orientations { get; }

        public FootprintGroup(ModAwareId id, IEnumerable<Footprint> footprints, IEnumerable<Angle> orientations)
        {
            Id = id;
            Footprints = footprints.ToImmutableArray();
            Orientations = orientations.ToImmutableArray();

            if (Footprints.Length == 0)
            {
                throw new ArgumentException("Footprint group must have at least one footprint.");
            }

            if (Footprints.Length != Orientations.Length)
            {
                throw new ArgumentException("Footprint group must have equal number footprints and orientations.");
            }
        }

        public PositionedFootprint Positioned(int index, Tile rootTile)
        {
            Argument.Satisfies(index >= 0 && index < Footprints.Length);
            return new PositionedFootprint(this, index, rootTile);
        }
    }
}
