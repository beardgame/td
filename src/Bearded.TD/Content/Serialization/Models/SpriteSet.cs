using Bearded.TD.Content.Models;

namespace Bearded.TD.Content.Serialization.Models
{
    class SpriteSet : IConvertsTo<Content.Models.SpriteSet, PackedSpriteSet>
    {
        public string Id { get; set; }
        public SpriteDrawGroup DrawGroup { get; set; } = SpriteDrawGroup.Unknown;
        public int DrawGroupOrderKey { get; set; }

        public Content.Models.SpriteSet ToGameModel(PackedSpriteSet sprites)
        {
            return new Content.Models.SpriteSet(Id, DrawGroup, DrawGroupOrderKey, sprites);
        }
    }
}
