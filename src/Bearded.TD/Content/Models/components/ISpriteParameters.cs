using amulware.Graphics;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models
{
    interface ISpriteParameters : IParametersTemplate<ISpriteParameters>
    {
        Color Color { get; }
        
        ISprite Sprite { get; }
    }
}
