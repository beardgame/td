using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;
using FootprintJson = Bearded.TD.Content.Serialization.Models.Footprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class FootprintBlueprintLoader : BaseBlueprintLoader<IFootprint, FootprintJson, Void>
{
    protected override string RelativePath => "defs/footprints";

    protected override DependencySelector SelectDependency => m => m.Blueprints.Footprints;

    public FootprintBlueprintLoader(BlueprintLoadingContext context) : base(context)
    {
    }

    protected override void SetupDependencyResolver(ReadonlyBlueprintCollection<IFootprint> blueprintCollection)
    {
        base.SetupDependencyResolver(blueprintCollection);

        Context.Serializer.Converters.Add(
            new PositionedFootprintConverter(Context.FindDependencyResolver<IFootprint>()));
    }
}
