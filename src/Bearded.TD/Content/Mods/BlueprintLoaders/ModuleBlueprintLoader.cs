using Bearded.TD.Game.Simulation.Enemies;
using Bearded.Utilities;
using ModuleBlueprintJson = Bearded.TD.Content.Serialization.Models.ModuleBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class ModuleBlueprintLoader : BaseBlueprintLoader<IModule, ModuleBlueprintJson, Void>
{
    protected override string RelativePath => "defs/modules";

    protected override DependencySelector SelectDependency => m => m.Blueprints.Modules;

    public ModuleBlueprintLoader(BlueprintLoadingContext context) : base(context) { }
}
