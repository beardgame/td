namespace Bearded.TD.Game.Generation
{
    enum LevelGenerationMethod : byte
    {
        // When adding values, make sure to support them in TilemapGenerator
        Legacy,
        Perlin,
        Empty,

        Default = Legacy
    }
}
