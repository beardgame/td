using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace Bearded.TD.Content.Serialization.Models.Fonts;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class FontDefinitionJson : IConvertsTo<FontDefinition, Void>
{
    public string Name { get; set; }
    public AtlasParametersJson Atlas { get; set; }
    public List<GlyphJson> Glyphs { get; set; }
    public List<KerningJson>? Kerning { get; set; }

    public FontDefinition ToGameModel(ModMetadata modMetadata, Void _)
    {
        return new FontDefinition(
            ModAwareId.FromNameInMod(Name ?? throw new InvalidDataException(), modMetadata),
            new Vector2(Atlas.Width, Atlas.Height) * (1 / (float)Atlas.DistanceRange),
            Glyphs.Select(g => g.ToGlyph(Atlas)),
            Kerning?.Select(k => ((char)k.Unicode1, (char)k.Unicode2, k.Advance))
                ?? Enumerable.Empty<(char, char, float)>()
        );
    }
}
