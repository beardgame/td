using Bearded.TD.Content.Models.Fonts;

namespace Bearded.TD.Content.Serialization.Models.Fonts;

readonly record struct GlyphJson(
    int Unicode,
    float Advance,
    Bounds PlaneBounds,
    Bounds AtlasBounds)
{
    public Glyph ToGlyph(AtlasParametersJson atlas)
    {
        return new Glyph(
            (char) Unicode,
            Advance,
            PlaneBounds,
            AtlasBounds.Multiply(1.0f / atlas.Width, 1.0f / atlas.Height));
    }
};
