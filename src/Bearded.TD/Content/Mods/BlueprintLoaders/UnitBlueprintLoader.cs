using Bearded.TD.Game.GameState.Units;
using Bearded.Utilities;
using UnitBlueprintJson = Bearded.TD.Content.Serialization.Models.UnitBlueprint;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    class UnitBlueprintLoader : BaseBlueprintLoader<IUnitBlueprint, UnitBlueprintJson, Void>
    {
        protected override string RelativePath => "defs/units";

        public UnitBlueprintLoader(BlueprintLoadingContext context) : base(context)
        {
        }
    }
}
