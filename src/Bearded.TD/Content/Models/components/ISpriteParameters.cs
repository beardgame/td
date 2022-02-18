using Bearded.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

enum SpriteColorMode
{
    Default = 0,
    Faction
}

interface ISpriteParameters : IParametersTemplate<ISpriteParameters>
{

    Color Color { get; }

    SpriteColorMode ColorMode { get;  }

    ISpriteBlueprint Sprite { get; }

    Shader? Shader { get; }

    SpriteDrawGroup? DrawGroup { get; }

    int DrawGroupOrderKey { get; }

    [Modifiable(1)]
    Unit Size { get; }

    Unit HeightOffset { get; }
}
