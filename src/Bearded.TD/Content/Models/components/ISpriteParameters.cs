using amulware.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface ISpriteParameters : IParametersTemplate<ISpriteParameters>
    {
        Color Color { get; }

        ISprite Sprite { get; }

        [Modifiable(0.2)]
        Unit Size { get; }
    }
}
