using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IFoundationParameters : IParametersTemplate<IFoundationParameters>
    {
        SpriteSet Sprites { get; }

        [Modifiable(0.15f)]
        Unit Height { get; }
    }
}
