using System.IO;
using Bearded.TD.Game;
using Model = Bearded.TD.Content.Models.Model;
using ModelJson = Bearded.TD.Content.Serialization.Models.Model;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class ModelBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<Model, ModelJson, (FileInfo, ModelLoader)>(context)
{
    protected override string RelativePath => "gfx/models";
    protected override DependencySelector SelectDependency => m => m.Blueprints.Models;

    private readonly ModelLoader loader = new(context.Context, context.Meta);

    protected override (FileInfo, ModelLoader) GetDependencyResolvers(FileInfo file)
    {
        return (file, loader);
    }

    protected override void SetupDependencyResolver(ReadonlyBlueprintCollection<Model> blueprintCollection)
    {
        base.SetupDependencyResolver(blueprintCollection);

        Context.AddDependencyResolver(new MeshResolver(Context.Meta, blueprintCollection, Context.LoadedDependencies));
    }
}
