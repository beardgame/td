using Bearded.Utilities;
using FootprintGroup = Bearded.TD.Game.Simulation.World.FootprintGroup;
using FootprintGroupJson = Bearded.TD.Content.Serialization.Models.FootprintGroup;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    sealed class FootprintGroupBlueprintLoader : BaseBlueprintLoader<FootprintGroup, FootprintGroupJson, Void>
    {
        protected override string RelativePath => "defs/footprints";

        protected override DependencySelector SelectDependency => m => m.Blueprints.Footprints;

        public FootprintGroupBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
        }
    }
}
