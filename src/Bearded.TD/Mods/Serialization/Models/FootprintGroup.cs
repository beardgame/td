using System.Collections.Generic;
using Bearded.TD.Game.Tiles;

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class FootprintGroup
    {
        public string Name { get; set; }
        public List<List<Step>> Footprints { get; set; }
    }
}
