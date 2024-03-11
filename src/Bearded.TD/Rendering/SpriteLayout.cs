using Bearded.Utilities.Geometry;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering;

enum SpriteSize
{
    FrameAgnostic = 0,
    StretchToFrame = 1,
    ContainInFrame = 2,
    CoverFrame = 3,
}

readonly record struct SpriteLayout(
    Rectangle Frame,
    SpriteSize Size,
    float Z,
    Vector2 FrameAlign,
    Vector2 SpriteAlign,
    Angle Angle,
    Vector2 Scale
)
{
    public SpriteLayout(
        Rectangle frame,
        SpriteSize size,
        float z = 0,
        Vector2? frameAlign = null,
        Vector2? spriteAlign = null,
        Angle angle = default,
        Vector2? scale = null)
        : this(
            frame,
            size,
            z,
            frameAlign ?? new Vector2(0.5f),
            spriteAlign ?? new Vector2(0.5f),
            angle,
            scale ?? Vector2.One
        )
    {
    }

    public static SpriteLayout CenteredAt(Vector3 position, float scale = 1, Angle angle = default)
        => new(
            new Rectangle(position.Xy, 0, 0),
            SpriteSize.FrameAgnostic,
            angle: angle,
            scale: new Vector2(scale),
            z: position.Z
        );

    public static SpriteLayout CenteredAt(Vector2 position, float scale = 1, Angle angle = default)
        => new(
            new Rectangle(position, 0, 0),
            SpriteSize.FrameAgnostic,
            angle: angle,
            scale: new Vector2(scale)
        );
}
