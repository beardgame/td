using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;
using FootprintJson = Bearded.TD.Content.Serialization.Models.Footprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class FootprintBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<IFootprint, FootprintJson, Void>(context)
{
    protected override string RelativePath => "defs/footprints";
    protected override DependencySelector SelectDependency => m => m.Blueprints.Footprints;

    protected override void SetupDependencyResolver(ReadonlyBlueprintCollection<IFootprint> blueprintCollection)
    {
        base.SetupDependencyResolver(blueprintCollection);

        Context.Serializer.Converters.Add(
            new PositionedFootprintConverter(Context.FindDependencyResolver<IFootprint>()));
    }
}
