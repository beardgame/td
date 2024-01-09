using Bearded.TD.Content.Models.Fonts;

namespace Bearded.TD.Content.Serialization.Models.Fonts;

readonly record struct Glyph(
    int Unicode,
    float Advance,
    Bounds PlaneBounds,
    Bounds AtlasBounds)
{
    public Content.Models.Fonts.Glyph ToGlyph(AtlasParameters atlas)
    {
        return new Content.Models.Fonts.Glyph(
            (char) Unicode,
            Advance,
            PlaneBounds,
            AtlasBounds.Multiply(1.0f / atlas.Width, 1.0f / atlas.Height));
    }
};
