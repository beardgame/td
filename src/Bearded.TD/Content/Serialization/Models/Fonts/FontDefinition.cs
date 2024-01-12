using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace Bearded.TD.Content.Serialization.Models.Fonts;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class FontDefinition : IConvertsTo<Content.Models.Fonts.FontDefinition, Void>
{
    public string Name { get; set; }
    public AtlasParameters Atlas { get; set; }
    public List<Glyph> Glyphs { get; set; }
    public List<Kerning>? Kerning { get; set; }

    public Content.Models.Fonts.FontDefinition ToGameModel(ModMetadata modMetadata, Void _)
    {
        return new Content.Models.Fonts.FontDefinition(
            ModAwareId.FromNameInMod(Name ?? throw new InvalidDataException(), modMetadata),
            new Vector2(Atlas.DistanceRange / (float)Atlas.Width, Atlas.DistanceRange / (float)Atlas.Height),
            Glyphs.Select(g => g.ToGlyph(Atlas)),
            Kerning?.Select(k => ((char)k.Unicode1, (char)k.Unicode2, k.Advance))
                ?? Enumerable.Empty<(char, char, float)>()
        );
    }
}
