using amulware.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface ISpotlightParameters : IParametersTemplate<ISpotlightParameters>
    {
        Color Color { get; }
        Unit Radius { get; }
        Angle Angle { get; }
    }
}
