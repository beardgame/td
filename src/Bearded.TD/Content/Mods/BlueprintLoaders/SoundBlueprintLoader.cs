using System.IO;
using Bearded.TD.Audio;
using SoundEffectJson = Bearded.TD.Content.Serialization.Models.SoundEffect;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class SoundBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<ISoundEffect, SoundEffectJson, (FileInfo, SoundLoader)>(context)
{
    protected override string RelativePath => "sfx";
    protected override DependencySelector SelectDependency => m => m.Blueprints.SoundEffects;

    private readonly SoundLoader loader = new(context.Meta);

    protected override (FileInfo, SoundLoader) GetDependencyResolvers(FileInfo file)
    {
        return (file, loader);
    }
}
