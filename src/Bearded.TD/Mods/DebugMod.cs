using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Buildings.Components;
using Bearded.TD.Game.Components.IPositionable;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Linq;
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
                FootprintGroup.Diamond,
                FootprintGroup.Line,
                FootprintGroup.Triangle
            });

            var componentFactories = new ReadonlyBlueprintCollection<ComponentFactory>(new []
            {
                new ComponentFactory("sink", () => new EnemySink()),
                new ComponentFactory("game_over_on_destroy", () => new GameOverOnDestroy()),
                new ComponentFactory("income_over_time", () => new IncomeOverTime()),
                new ComponentFactory("turret", () => new Turret(), () => new TileVisibility(Turret.Range)),
                new ComponentFactory("worker_hub", () => new WorkerHub())
            });

            var buildings = new ReadonlyBlueprintCollection<BuildingBlueprint>(new[]
            {
                new BuildingBlueprint("base", FootprintGroup.CircleSeven, 1000, 1,
                    new[]
                    {
                        componentFactories["sink"],
                        componentFactories["income_over_time"],
                        componentFactories["game_over_on_destroy"],
                        componentFactories["worker_hub"],
                    }),
                new BuildingBlueprint("wall", FootprintGroup.Single, 100, 15, null),
                new BuildingBlueprint("triangle", FootprintGroup.Triangle, 300,
                    75, componentFactories["turret"].Yield()),
                new BuildingBlueprint("diamond", FootprintGroup.Diamond, 200, 40, null),
                new BuildingBlueprint("line", FootprintGroup.Line, 150, 25, null)
            });

            var enemies = new ReadonlyBlueprintCollection<UnitBlueprint>(new[]
            {
                new UnitBlueprint("debug", 100, 10, 2.S(), new Speed(2), 2, Color.DarkRed),
                new UnitBlueprint("strong", 250, 20, 1.5.S(), new Speed(1.2f), 4, Color.Yellow),
                new UnitBlueprint("fast", 50, 4, .5.S(), new Speed(3), 4, Color.CornflowerBlue),
                new UnitBlueprint("tank", 1000, 50, 2.S(), new Speed(.8f), 12, Color.SandyBrown)
            });

            return new Mod(
                footprints,
                componentFactories,
                buildings, 
                enemies);
        }
    }
}
