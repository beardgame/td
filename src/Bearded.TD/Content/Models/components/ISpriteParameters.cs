using Bearded.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface ISpriteParameters : IParametersTemplate<ISpriteParameters>
    {
        Color Color { get; }

        ISpriteBlueprint Sprite { get; }

        [Modifiable(1)]
        Unit Size { get; }

        Unit HeightOffset { get; }
    }
}
