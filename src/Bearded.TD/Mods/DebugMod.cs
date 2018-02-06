using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Mods.Models;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

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

            var enemies = new ReadonlyBlueprintCollection<UnitBlueprint>(new[]
            {
                new UnitBlueprint("debug0", "debug", 100, 10, 2.S(), new Speed(2), 2, Color.DarkRed),
                new UnitBlueprint("strong0", "strong", 250, 20, 1.5.S(), new Speed(1.2f), 4, Color.Yellow),
                new UnitBlueprint("fast0", "fast", 50, 4, .5.S(), new Speed(3), 4, Color.CornflowerBlue),
                new UnitBlueprint("tank0", "tank", 1000, 50, 2.S(), new Speed(.8f), 12, Color.SandyBrown)
            });

            return new Mod(
                footprints,
                buildings, 
                enemies);
        }
    }
}
