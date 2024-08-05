using System.IO;
using Bearded.TD.Content.Mods;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed record Model(
    string? Id
) : IConvertsTo<Content.Models.Model, (FileInfo, ModelLoader)>
{
    public Content.Models.Model ToGameModel(ModMetadata modMetadata, (FileInfo, ModelLoader) resolvers)
    {
        var (file, loader) = resolvers;

        return loader.TryLoad(file, this);
    }
}
