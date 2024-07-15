using System;

namespace Bearded.TD.UI.Shapes;

[Flags]
enum GradientFlags : ushort
{
    Default = 0,
    GlowFade = 1,
    Dither = 2,
    ExtendNegative = 4,
    ExtendPositive = 8,
    ExtendBoth = ExtendNegative | ExtendPositive,
    Repeat = 16,
}
