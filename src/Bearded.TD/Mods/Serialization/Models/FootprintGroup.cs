using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class FootprintGroup
    {
        public string Name { get; set; }
        public List<List<Step>> Footprints { get; set; }

        public Tiles.FootprintGroup ToGameModel()
        {
            return new Tiles.FootprintGroup(Name,
                Footprints.Select(steps => new Footprint(steps))
                );
        }
    }
}
