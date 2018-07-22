using Bearded.TD.Mods.Models;
using Bearded.Utilities;

namespace Bearded.TD.Mods.Serialization.Models
{
    class SpriteSet : IConvertsTo<Mods.Models.SpriteSet, Void>
    {
        public string Id { get; set; }
        public SpriteDrawGroup DrawGroup { get; set; }
        public int DrawGroupOrderKey { get; set; }

        public Mods.Models.SpriteSet ToGameModel(Void resolvers)
        {
            return new Mods.Models.SpriteSet(Id, DrawGroup, DrawGroupOrderKey);
        }
    }
}
