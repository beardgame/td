using Bearded.UI;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Shapes;

readonly record struct AnchorPoint(
    Vector2 AbsoluteOffset,
    Vector2 RelativeOffset,
    Vector2 RelativeContribution)
{
    public AnchorPoint(Vector2 absoluteOffset = default, Vector2 relativeOffset = default)
        : this(AbsoluteOffset: absoluteOffset, RelativeOffset: relativeOffset, RelativeContribution: Vector2.One) { }

    public Vector2 CalculatePointWithin(Frame frame)
        => ((Vector2)frame.TopLeft + (Vector2)frame.Size * RelativeOffset) * RelativeContribution + AbsoluteOffset;

    public static AnchorPoint Absolute(Vector2 point) => new(absoluteOffset: point);

    public static AnchorPoint Relative(Vector2 point) => new(relativeOffset: point);

    public static implicit operator AnchorPoint(Vector2 point) => Absolute(point);

    public override string ToString()
        => $"(tl + wh * {RelativeOffset}) * {RelativeContribution} + {AbsoluteOffset}";
}
