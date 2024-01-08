namespace Bearded.TD.Content.Models.Fonts;

readonly record struct Glyph(
    char Character,
    float Advance,
    Bounds VertexBounds,
    Bounds UVBounds);
