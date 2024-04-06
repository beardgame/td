namespace Bearded.TD.UI.Shapes;

enum GradientType : byte
{
    None = 0,

    // Single Color
    Constant = 1,

    // Full Gradients
    Linear = 20,
    RadialWithRadius = 21,
    RadialToPoint = 22,
    AlongEdgeNormal = 23,
    ArcAroundPoint = 24,
}

enum GradientTypeSingleColor : byte
{
    Constant = GradientType.Constant,
}

enum ComponentBlendMode : byte
{
    PremultipliedAdd = 0,
    Multiply = 1,
}
