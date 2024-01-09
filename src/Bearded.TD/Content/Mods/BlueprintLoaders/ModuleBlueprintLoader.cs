using Bearded.TD.Game.Simulation.Enemies;
using Bearded.Utilities;
using ModuleBlueprintJson = Bearded.TD.Content.Serialization.Models.ModuleBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class ModuleBlueprintLoader(BlueprintLoadingContext context)
    : BaseBlueprintLoader<IModule, ModuleBlueprintJson, Void>(context)
{
    protected override string RelativePath => "defs/modules";
    protected override DependencySelector SelectDependency => m => m.Blueprints.Modules;
}
