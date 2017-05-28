using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Tiles
{
    sealed class FootprintGroup : IIdable<FootprintGroup>
    {
        public static FootprintGroup Single { get; } = new FootprintGroup(new Id<FootprintGroup>(1), Footprint.Single);
        public static FootprintGroup Triangle { get; } = new FootprintGroup(new Id<FootprintGroup>(2), Footprint.TriangleUp, Footprint.TriangleDown);
        public static FootprintGroup CircleSeven { get; } = new FootprintGroup(new Id<FootprintGroup>(3), Footprint.CircleSeven);

        public Id<FootprintGroup> Id { get; }
        public ReadOnlyCollection<Footprint> Footprints { get; }

        public FootprintGroup(Id<FootprintGroup> id, IEnumerable<Footprint> footprints)
        {
            Id = id;
            Footprints = footprints.ToList().AsReadOnly();

            if (Footprints.Count == 0)
                throw new ArgumentException("Footprint group must have at least one footprint.");
        }

        public FootprintGroup(Id<FootprintGroup> id, params Footprint[] footprints)
            : this(id, (IEnumerable<Footprint>)footprints)
        {
        }
    }
}
