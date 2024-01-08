using System;
using System.Runtime.CompilerServices;
using Bearded.TD.Content.Models.Fonts;

namespace Bearded.TD.Rendering.Text;

static class TextLayout
{
    public static float LineWidth(string text, IFontDefinition font)
    {
        var advanced = 0f;
        var previous = (char)0;

        foreach (var current in text)
        {
            applyKerning(font, previous, current, ref advanced);
            advance(font, current, ref advanced);
            previous = current;
        }

        return advanced;
    }

    public static GlyphLine LayoutLine(string text, IFontDefinition font)
    {
        return LayoutLine(text, font, new LaidOutGlyph[text.Length]);
    }

    public static GlyphLine LayoutLine(string text, IFontDefinition font, Span<LaidOutGlyph> glyphSpan)
    {
        if (text.Length != glyphSpan.Length)
            throw new ArgumentException("Span must be of same length as text.");

        if (text.Length == 0)
            return new GlyphLine(glyphSpan, 0);

        var advanced = 0f;
        var previous = (char)0;

        for (var i = 0; i < text.Length; i++)
        {
            var current = text[i];
            applyKerning(font, previous, current, ref advanced);
            layoutAndAdvance(font, current, out glyphSpan[i], ref advanced);
            previous = current;
        }

        return new GlyphLine(glyphSpan, advanced);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void applyKerning(IFontDefinition font, char from, char to, ref float advanced)
    {
        if (font.Kerning.TryGetValue((from, to), out var kern))
            advanced += kern;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void layoutAndAdvance(IFontDefinition font,
        char c,
        out LaidOutGlyph laidOutGlyph,
        ref float advanced)
    {
        if (!font.Glyphs.TryGetValue(c, out var glyph))
        {
            laidOutGlyph = default;
            return;
        }
        laidOutGlyph = new LaidOutGlyph(
            glyph.VertexBounds.TranslateX(advanced),
            glyph.UVBounds);
        advanced += glyph.Advance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void advance(IFontDefinition font, char c, ref float advanced)
    {
        if (font.Glyphs.TryGetValue(c, out var glyph))
            advanced += glyph.Advance;
    }
}
