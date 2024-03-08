using System;

namespace Bearded.TD.Rendering.Shapes;

readonly struct GradientId(uint value)
{
    public static GradientId None => default;
    public readonly uint Value = value < 0xFFFFFF ? value : throw new ArgumentOutOfRangeException(nameof(value));

    public bool IsNone => Value == 0;
}
