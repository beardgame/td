using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.GameState;

namespace Bearded.TD.Content.Models
{
    enum SpriteDrawGroup
    {
        // When adding new groups, make sure the DeferredRenderer knows about them, or they won't render
        LevelDetail,
        Building,
        Unit,
        Particle,
        Unknown
    }

    sealed class SpriteSet : IBlueprint
    {
        public ModAwareId Id { get; }
        public SpriteDrawGroup DrawGroup { get; }
        public int DrawGroupOrderKey { get; }
        public PackedSpriteSet Sprites { get; }

        public SpriteSet(ModAwareId id, SpriteDrawGroup drawGroup, int drawGroupOrderKey, PackedSpriteSet sprites)
        {
            Id = id;
            DrawGroup = drawGroup;
            DrawGroupOrderKey = drawGroupOrderKey;
            Sprites = sprites;
        }
    }
}
