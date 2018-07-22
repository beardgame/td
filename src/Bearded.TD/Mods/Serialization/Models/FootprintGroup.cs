using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.Utilities;
// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class FootprintGroup : IConvertsTo<Game.World.FootprintGroup, Void>
    {
        public string Id { get; set; }
        public List<List<Step>> Footprints { get; set; }

        public Game.World.FootprintGroup ToGameModel(Void _)
        {
            return new Game.World.FootprintGroup(Id,
                Footprints.Select(steps => new Footprint(steps))
                );
        }
    }
}
