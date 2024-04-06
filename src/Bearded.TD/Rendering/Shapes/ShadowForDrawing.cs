using Bearded.TD.UI.Shapes;
using Bearded.UI;

namespace Bearded.TD.Rendering.Shapes;

readonly record struct ShadowForDrawing(
    Shadow Shadow,
    IShapeComponentBuffer ShapeBuffer,
    ShapeComponents OverlayComponents,
    (IGradientBuffer, Frame)? GradientsInFrame
);
