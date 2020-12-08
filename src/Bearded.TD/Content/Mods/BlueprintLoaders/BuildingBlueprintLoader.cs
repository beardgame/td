using System.IO;
using Bearded.TD.Game.GameState.Buildings;
using BuildingBlueprintJson = Bearded.TD.Content.Serialization.Models.BuildingBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class BuildingBlueprintLoader : BaseBlueprintLoader<IBuildingBlueprint, BuildingBlueprintJson, UpgradeTagResolver>
    {
        private readonly UpgradeTagResolver tagResolver;
        protected override string RelativePath => "defs/buildings";

        protected override DependencySelector SelectDependency => m => m.Blueprints.Buildings;

        public BuildingBlueprintLoader(BlueprintLoadingContext context, UpgradeTagResolver tagResolver)
            : base(context)
        {
            this.tagResolver = tagResolver;
        }

        protected override UpgradeTagResolver GetDependencyResolvers(FileInfo file)
        {
            return tagResolver;
        }
    }
}
