using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Tiles
{
    sealed class FootprintGroup : IIdable<FootprintGroup>, INamed
    {
        public static FootprintGroup Single { get; } =
            new FootprintGroup(new Id<FootprintGroup>(1), "single", Footprint.Single);
        public static FootprintGroup Triangle { get; } = new FootprintGroup(new Id<FootprintGroup>(2), "triangle",
            Footprint.TriangleUp, Footprint.TriangleDown);
        public static FootprintGroup CircleSeven { get; } =
            new FootprintGroup(new Id<FootprintGroup>(3), "circleseven", Footprint.CircleSeven);

        public Id<FootprintGroup> Id { get; }
        public string Name { get; }
        public ReadOnlyCollection<Footprint> Footprints { get; }

        public FootprintGroup(Id<FootprintGroup> id, string name, IEnumerable<Footprint> footprints)
        {
            Id = id;
            Name = name;
            Footprints = footprints.ToList().AsReadOnly();

            if (Footprints.Count == 0)
                throw new ArgumentException("Footprint group must have at least one footprint.");
        }

        public FootprintGroup(Id<FootprintGroup> id, string name, params Footprint[] footprints)
            : this(id, name, (IEnumerable<Footprint>)footprints)
        {
        }
    }
}
