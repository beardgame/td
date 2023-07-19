using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.Utilities;
using UpgradeBlueprintJson = Bearded.TD.Content.Serialization.Models.UpgradeBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders;

sealed class UpgradeBlueprintLoader : BaseBlueprintLoader<IPermanentUpgrade, UpgradeBlueprintJson, Void>
{
    protected override string RelativePath => "defs/upgrades";

    protected override DependencySelector SelectDependency => m => m.Blueprints.Upgrades;

    public UpgradeBlueprintLoader(BlueprintLoadingContext context) : base(context)
    {
    }
}