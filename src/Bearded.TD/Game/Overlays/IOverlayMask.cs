using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using static Bearded.TD.UI.Shapes.GradientDefinition;

namespace Bearded.TD.Game.Overlays;

interface IOverlayMask
{
    ShapeComponent Mask { get; }
}

sealed class FadedCircleMask(IPositionable center, float innerRadius, float fadeRadius)
    : IOverlayMask
{
    private readonly ImmutableArray<GradientStop> gradient =
    [
        (innerRadius / (innerRadius + fadeRadius), Color.White),
        (1, Color.Transparent),
    ];

    private GradientDefinition shape =>
        Radial(AnchorPoint.Absolute(center.Position.XY().NumericValue), innerRadius + fadeRadius)
            .WithBlendMode(ComponentBlendMode.Multiply);

    public ShapeComponent Mask =>
        new(float.NegativeInfinity, float.PositiveInfinity, ShapeColor.From(gradient, shape));
}
