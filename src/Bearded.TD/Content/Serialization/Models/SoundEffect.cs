using System.IO;
using Bearded.TD.Audio;
using Bearded.TD.Content.Mods;
using JetBrains.Annotations;

namespace Bearded.TD.Content.Serialization.Models;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
sealed class SoundEffect : IConvertsTo<Content.Models.SoundEffect, (FileInfo, SoundLoader)>
{
    public string? Id { get; set; }
    public PitchRange? PitchRange { get; set; }

    public Content.Models.SoundEffect ToGameModel(ModMetadata modMetadata, (FileInfo, SoundLoader) resolvers)
    {
        var (file, loader) = resolvers;

        return loader.Load(file, this);
    }
}
