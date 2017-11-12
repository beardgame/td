using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class FootprintGroup
    {
        public string Id { get; set; }
        public List<List<Step>> Footprints { get; set; }

        public Mods.Models.FootprintGroup ToGameModel()
        {
            return new Mods.Models.FootprintGroup(Id,
                Footprints.Select(steps => new Footprint(steps))
                );
        }
    }
}
