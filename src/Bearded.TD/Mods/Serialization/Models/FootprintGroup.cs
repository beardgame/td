using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class FootprintGroup : IConvertsTo<Mods.Models.FootprintGroup, Void>
    {
        public string Name { get; set; }
        public List<List<Step>> Footprints { get; set; }

        public Mods.Models.FootprintGroup ToGameModel(Void _)
        {
            return new Mods.Models.FootprintGroup(Name,
                Footprints.Select(steps => new Footprint(steps))
                );
        }
    }
}
