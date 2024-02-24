using Bearded.TD.Content.Models.Fonts;

namespace Bearded.TD.Rendering.Text;

readonly record struct LaidOutGlyph(
    Bounds VertexBounds,
    Bounds UVBounds,
    float Advanced);
