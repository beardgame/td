namespace Bearded.TD.UI.Shapes;

enum GradientType : byte
{
    None = 0,

    // Single Color
    Constant = 1,
    SimpleGlow = 2,

    // Full Gradients
    Linear = 20,
    Radial = 21,
    AlongEdgeNormal = 22,
}

enum GradientTypeSingleColor : byte
{
    None = 0,

    Constant = GradientType.Constant,
    SimpleGlow = GradientType.SimpleGlow,
}
