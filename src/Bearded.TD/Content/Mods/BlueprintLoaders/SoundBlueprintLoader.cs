using System.IO;
using Bearded.TD.Audio;
using SoundEffectJson = Bearded.TD.Content.Serialization.Models.SoundEffect;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class SoundBlueprintLoader : BaseBlueprintLoader<ISoundEffect, SoundEffectJson, (FileInfo, SoundLoader)>
{
    private readonly SoundLoader loader;

    protected override string RelativePath => "sfx";

    protected override DependencySelector? SelectDependency { get; } =  m => m.Blueprints.SoundEffects;

    public SoundBlueprintLoader(BlueprintLoadingContext context) : base(context)
    {
        loader = new SoundLoader(context.Context, context.Meta);
    }

    protected override (FileInfo, SoundLoader) GetDependencyResolvers(FileInfo file)
    {
        return (file, loader);
    }
}