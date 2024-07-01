namespace Bearded.TD.Content.Models;

enum DrawOrderGroup
{
    // When adding new groups, make sure the LayerRenderer or DeferredRenderer know about them, or they won't render
    Unknown,
    Level,
    SolidLevelDetails,

    LevelProjected,

    LevelDetail,
    Building,
    Unit,
    Fluids,
    Particle,
    IgnoreDepth,

    UIBackground,
    UIShapes,
    UISpritesBack,
    UIFont,
    UISpritesTop,
}
