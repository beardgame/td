using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using OpenTK.Mathematics;

namespace Bearded.TD.Content.Models.Fonts;

interface IFontDefinition
{
    float CapHeight { get; }
    Vector2 UnitRange { get; }
    IReadOnlyDictionary<char, Glyph> Glyphs { get; }
    IReadOnlyDictionary<(char, char), float> Kerning { get; }
}

sealed class FontDefinition : IFontDefinition, IBlueprint
{
    public ModAwareId Id { get; }
    public float CapHeight { get; }
    public Vector2 UnitRange { get; }
    public IReadOnlyDictionary<char, Glyph> Glyphs { get; }
    public IReadOnlyDictionary<(char, char), float> Kerning { get; }

    public FontDefinition(
        ModAwareId id,
        float capHeight,
        Vector2 unitRange,
        IEnumerable<Glyph> glyphs,
        IEnumerable<(char C1, char C2, float Advance)> kerning)
    {
        Id = id;
        CapHeight = capHeight;
        UnitRange = unitRange;
        Glyphs = glyphs.ToDictionary(g => g.Character);
        Kerning = kerning.ToDictionary(k => (k.C1, k.C2), k => k.Advance);
    }
}
