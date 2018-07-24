using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods.Serialization.Models
{
    class SpriteSet : IConvertsTo<Mods.Models.SpriteSet, PackedSpriteSet>
    {
        public string Id { get; set; }
        public SpriteDrawGroup DrawGroup { get; set; } = SpriteDrawGroup.Unknown;
        public int DrawGroupOrderKey { get; set; }

        public Mods.Models.SpriteSet ToGameModel(PackedSpriteSet sprites)
        {
            return new Mods.Models.SpriteSet(Id, DrawGroup, DrawGroupOrderKey, sprites);
        }
    }
}
