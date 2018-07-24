using Bearded.TD.Game;

namespace Bearded.TD.Mods.Models
{
    enum SpriteDrawGroup
    {
        Building,
        Unit,
        Particle
    }

    class SpriteSet : IBlueprint
    {
        public string Id { get; }
        public SpriteDrawGroup DrawGroup { get; }
        public int DrawGroupOrderKey { get; }
        public PackedSpriteSet Sprites { get; }

        public SpriteSet(string id, SpriteDrawGroup drawGroup, int drawGroupOrderKey, PackedSpriteSet sprites)
        {
            Id = id;
            DrawGroup = drawGroup;
            DrawGroupOrderKey = drawGroupOrderKey;
            Sprites = sprites;
        }
    }
}
