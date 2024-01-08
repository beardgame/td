using System;

namespace Bearded.TD.Rendering.Text;

readonly ref struct GlyphLine(Span<LaidOutGlyph> glyphs, float width)
{
    public Span<LaidOutGlyph> Glyphs { get; } = glyphs;
    public float Width { get; } = width;
}
