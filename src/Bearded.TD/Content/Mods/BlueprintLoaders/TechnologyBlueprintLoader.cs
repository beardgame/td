using Bearded.TD.Game.Technologies;
using Bearded.Utilities;
using TechnologyBlueprintJson = Bearded.TD.Content.Serialization.Models.TechnologyBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class TechnologyBlueprintLoader : BaseBlueprintLoader<ITechnologyBlueprint, TechnologyBlueprintJson, Void>
    {
        protected override string RelativePath => "defs/technologies";

        public TechnologyBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
        }
    }
}
