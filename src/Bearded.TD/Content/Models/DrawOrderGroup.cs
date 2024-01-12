namespace Bearded.TD.Content.Models;

enum DrawOrderGroup
{
    // When adding new groups, make sure the DeferredRenderer knows about them, or they won't render
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
