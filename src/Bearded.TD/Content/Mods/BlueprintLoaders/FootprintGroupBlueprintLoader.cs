using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.Utilities;
using FootprintGroup = Bearded.TD.Game.World.FootprintGroup;
using FootprintGroupJson = Bearded.TD.Content.Serialization.Models.FootprintGroup;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class FootprintGroupBlueprintLoader : BaseBlueprintLoader<FootprintGroup, FootprintGroupJson, Void>
    {
        protected override string RelativePath => "defs/footprints";

        protected override DependencySelector SelectDependency => m => m.Blueprints.Footprints;

        public FootprintGroupBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
        }

        protected override void SetupDependencyResolver(ReadonlyBlueprintCollection<FootprintGroup> blueprintCollection)
        {
            base.SetupDependencyResolver(blueprintCollection);

            Context.Serializer.Converters.Add(
                new PositionedFootprintConverter(Context.FindDependencyResolver<FootprintGroup>()));
        }
    }
}
