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

        public SpriteSet(string id, SpriteDrawGroup drawGroup, int drawGroupOrderKey)
        {
            Id = id;
            DrawGroup = drawGroup;
            DrawGroupOrderKey = drawGroupOrderKey;
        }
    }
}
