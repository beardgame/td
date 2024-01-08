using System.IO;
using Bearded.TD.Content.Models.Fonts;
using Bearded.TD.Content.Mods;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models.Fonts;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class Font : IConvertsTo<Bearded.TD.Content.Models.Fonts.Font, int>
{
    public string Id { get; set; }
    public FontDefinition Definition { get; set; }
    public Content.Models.Material Material { get; set; }

    public Content.Models.Fonts.Font ToGameModel(ModMetadata modMetadata, int resolvers)
    {
        return new Content.Models.Fonts.Font(
            ModAwareId.FromNameInMod(Id ?? throw new InvalidDataException(), modMetadata),
            Definition,
            Material
        );
    }
}
