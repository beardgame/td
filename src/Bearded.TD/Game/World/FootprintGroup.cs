using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    sealed class FootprintGroup : IBlueprint
    {
        public string Id { get; }

        public static FootprintGroup Single { get; } = new FootprintGroup("single0", Footprint.Single);
        public static FootprintGroup Triangle { get; }
            = new FootprintGroup("triangle0", Footprint.TriangleUp, Footprint.TriangleDown);
        public static FootprintGroup CircleSeven { get; } = new FootprintGroup("seven0", Footprint.CircleSeven);
        
        public ReadOnlyCollection<Footprint> Footprints { get; }

        public FootprintGroup(string id, IEnumerable<Footprint> footprints)
        {
            Id = id;
            Footprints = footprints.ToList().AsReadOnly();

            if (Footprints.Count == 0)
                throw new ArgumentException("Footprint group must have at least one footprint.");
        }

        public FootprintGroup(string name, params Footprint[] footprints)
            : this(name, (IEnumerable<Footprint>)footprints)
        {
        }

        public PositionedFootprint Positioned(int index, Level level, Position2 position)
        {
            DebugAssert.Argument.Satisfies(index >= 0 && index < Footprints.Count);

            return new PositionedFootprint(level, this, index,
                Footprints[index].RootTileClosestToWorldPosition(level, position));
        }
    }
}
