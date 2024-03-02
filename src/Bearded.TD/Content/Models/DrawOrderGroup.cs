namespace Bearded.TD.Content.Models;

enum DrawOrderGroup
{
    // When adding new groups, make sure the LayerRenderer or DeferredRenderer know about them, or they won't render
    Unknown,
    SolidLevelDetails,
    LevelDetail,
    Building,
    Unit,
    Particle,
    IgnoreDepth,

    UIBackground,
    UISpritesBack,
    UIFont,
    UISpritesTop,
}
