using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Mods.Models;
using Bearded.Utilities.Linq;

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
                new ComponentFactory("gameOverOnDestroy", () => new GameOverOnDestroy<Building>()),
                new ComponentFactory("incomeOverTime", () => new IncomeOverTime<Building>(new IncomeOverTimeParameters(5))),
                new ComponentFactory("turret",
                    () => new Turret(new TurretParameters()),
                    () => new TileVisibility<BuildingGhost>(new TileVisibilityParameters(new TurretParameters().Range))),
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

            var enemies = new ReadonlyBlueprintCollection<UnitBlueprint>(Enumerable.Empty<UnitBlueprint>());

            return new Mod(
                footprints,
                componentFactories,
                buildings, 
                enemies);
        }
    }
}
