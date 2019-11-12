using amulware.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IPointLightParameters : IParametersTemplate<IPointLightParameters>
    {
        Color Color { get; }
        Unit Radius { get; }
    }
}
