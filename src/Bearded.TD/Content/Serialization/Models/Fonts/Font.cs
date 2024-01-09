using System.IO;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models.Fonts;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class Font : IConvertsTo<Bearded.TD.Content.Models.Fonts.Font, Void>
{
    public string Id { get; set; }
    public Content.Models.Fonts.FontDefinition Definition { get; set; }
    public Content.Models.Material Material { get; set; }

    public Content.Models.Fonts.Font ToGameModel(ModMetadata modMetadata, Void _)
    {
        return new Content.Models.Fonts.Font(
            ModAwareId.FromNameInMod(Id ?? throw new InvalidDataException(), modMetadata),
            Definition,
            Material
        );
    }
}
