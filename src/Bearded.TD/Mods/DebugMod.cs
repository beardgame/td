using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Buildings.Components;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Mods.Models;
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
                FootprintGroup.Triangle
            });

            var componentFactories = new ReadonlyBlueprintCollection<ComponentFactory>(new []
            {
                new ComponentFactory("sink", () => new EnemySink()),
                new ComponentFactory("gameOverOnDestroy", () => new GameOverOnDestroy()),
                new ComponentFactory("incomeOverTime", () => new IncomeOverTime<Building>(new IncomeOverTimeParameters(5))),
                new ComponentFactory("turret", () => new Turret(), () => new TileVisibility<BuildingGhost>(new TileVisibilityParameters(Turret.Range))),
                new ComponentFactory("workerHub", () => new WorkerHub<Building>(new WorkerHubParameters(2)))
            });

            var buildings = new ReadonlyBlueprintCollection<BuildingBlueprint>(new[]
            {
                new BuildingBlueprint("base0", "base", FootprintGroup.CircleSeven, 1000, 1,
                    new[]
                    {
                        componentFactories["sink"],
                        componentFactories["incomeOverTime"],
                        componentFactories["gameOverOnDestroy"],
                        componentFactories["workerHub"],
                    }),
                new BuildingBlueprint("wall0", "wall", FootprintGroup.Single, 100, 15, null),
                new BuildingBlueprint("triangle0", "triangle", FootprintGroup.Triangle, 300,
                    75, componentFactories["turret"].Yield()),
            });

            var enemies = new ReadonlyBlueprintCollection<UnitBlueprint>(new[]
            {
                new UnitBlueprint("debug0", "debug", 100, 10, 2.S(), new Speed(2), 2, Color.DarkRed),
                new UnitBlueprint("strong0", "strong", 250, 20, 1.5.S(), new Speed(1.2f), 4, Color.Yellow),
                new UnitBlueprint("fast0", "fast", 50, 4, .5.S(), new Speed(3), 4, Color.CornflowerBlue),
                new UnitBlueprint("tank0", "tank", 1000, 50, 2.S(), new Speed(.8f), 12, Color.SandyBrown)
            });

            return new Mod(
                footprints,
                componentFactories,
                buildings, 
                enemies);
        }
    }
}
