using System.Linq;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Mods
{
    static class DebugMod
    {
        public static Mod Create()
        {
            var footprints = new ReadonlyBlueprintCollection<FootprintGroup>(new []
            {
                FootprintGroup.Single,
                FootprintGroup.CircleSeven,
                FootprintGroup.Triangle
            });
            
            var buildings = new ReadonlyBlueprintCollection<BuildingBlueprint>(Enumerable.Empty<BuildingBlueprint>());

            var enemies = new ReadonlyBlueprintCollection<UnitBlueprint>(Enumerable.Empty<UnitBlueprint>());

            return new Mod(
                footprints,
                buildings, 
                enemies);
        }
    }
}
