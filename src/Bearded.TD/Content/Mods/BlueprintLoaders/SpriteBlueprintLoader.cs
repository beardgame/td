using System.IO;
using Bearded.TD.Game;
using SpriteSet = Bearded.TD.Content.Models.SpriteSet;
using SpriteSetJson = Bearded.TD.Content.Serialization.Models.SpriteSet;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class SpriteBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<SpriteSet, SpriteSetJson, (FileInfo, SpriteSetLoader)>(context)
{
    protected override string RelativePath => "gfx/sprites";
    protected override DependencySelector SelectDependency => m => m.Blueprints.Sprites;

    private readonly SpriteSetLoader loader = new(context.Context, context.Meta);

    protected override void SetupDependencyResolver(ReadonlyBlueprintCollection<SpriteSet> blueprintCollection)
    {
        base.SetupDependencyResolver(blueprintCollection);

        Context.AddDependencyResolver(
            new SpriteResolver(Context.Meta, blueprintCollection, Context.LoadedDependencies));
    }

    protected override (FileInfo, SpriteSetLoader) GetDependencyResolvers(FileInfo file)
    {
        return (file, loader);
    }
}
