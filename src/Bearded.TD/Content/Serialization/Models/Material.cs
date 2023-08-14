using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Mods;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class Material : IConvertsTo<Content.Models.Material, (FileInfo, MaterialLoader)>
{
    public string? Id { get; set; }

    public Content.Models.Shader? Shader { get; set; }

    public List<NamedTextureFile>? Textures { get; set; }

    public Content.Models.Material ToGameModel(ModMetadata modMetadata, (FileInfo, MaterialLoader) resolvers)
    {
        _ = Id ?? throw new InvalidDataException($"{nameof(Id)} must be non-null");
        _ = Shader ?? throw new InvalidDataException($"{nameof(Shader)} must be non-null");

        var (file, loader) = resolvers;
        var textures = (Textures ?? Enumerable.Empty<NamedTextureFile>())
            .Select(t => (Name: t.Sampler, loader.LoadTextureImage(file, t.File)))
            .ToList().AsReadOnly();

        return new Content.Models.Material(ModAwareId.FromNameInMod(Id, modMetadata), Shader, textures);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public record NamedTextureFile(string Sampler, string File);
}
