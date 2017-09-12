using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bearded.TD.Game.Tiles
{
    sealed class FootprintGroup
    {
        public static FootprintGroup Single { get; } = new FootprintGroup(Footprint.Single);
        public static FootprintGroup Triangle { get; }
            = new FootprintGroup(Footprint.TriangleUp, Footprint.TriangleDown);
        public static FootprintGroup CircleSeven { get; } = new FootprintGroup(Footprint.CircleSeven);
        public static FootprintGroup Diamond { get; }
            = new FootprintGroup(Footprint.DiamondBottomLeftTopRight, Footprint.DiamondTopBottom, Footprint.DiamondTopLeftBottomRight);
        public static FootprintGroup Line { get; }
            = new FootprintGroup(Footprint.LineUp, Footprint.LineStraight, Footprint.LineDown);
        
        public ReadOnlyCollection<Footprint> Footprints { get; }

        public FootprintGroup(IEnumerable<Footprint> footprints)
        {
            Footprints = footprints.ToList().AsReadOnly();

            if (Footprints.Count == 0)
                throw new ArgumentException("Footprint group must have at least one footprint.");
        }

        public FootprintGroup(params Footprint[] footprints)
            : this((IEnumerable<Footprint>)footprints)
        {
        }
    }
}
