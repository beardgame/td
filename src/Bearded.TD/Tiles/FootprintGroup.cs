using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tiles
{
    sealed class FootprintGroup : IBlueprint
    {
        public string Name { get; }

        public static FootprintGroup Single { get; } = new FootprintGroup("singe", Footprint.Single);
        public static FootprintGroup Triangle { get; }
            = new FootprintGroup("triangle", Footprint.TriangleUp, Footprint.TriangleDown);
        public static FootprintGroup CircleSeven { get; } = new FootprintGroup("seven", Footprint.CircleSeven);
        public static FootprintGroup Diamond { get; }
            = new FootprintGroup("diamond", Footprint.DiamondBottomLeftTopRight, Footprint.DiamondTopBottom, Footprint.DiamondTopLeftBottomRight);
        public static FootprintGroup Line { get; }
            = new FootprintGroup("line", Footprint.LineUp, Footprint.LineStraight, Footprint.LineDown);
        
        public ReadOnlyCollection<Footprint> Footprints { get; }

        public FootprintGroup(string name, IEnumerable<Footprint> footprints)
        {
            Name = name;
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
