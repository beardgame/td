using System;
using Bearded.TD.Game.Commands.Loading;
using Bearded.TD.Game.Simulation.Buildings;
using static Bearded.TD.Game.Generation.Semantic.Commands.LevelGenerationCommand;

namespace Bearded.TD.Game.Generation.Semantic.Commands
{
    static class LevelGenerationCommandConverter
    {
        public static CommandFactory ToCommandFactory(LevelGenerationCommand input)
        {
            return input switch
            {
                PlaceBuilding(var blueprint, var footprint) =>
                    gameInstance => PlopBuilding.Command(
                        gameInstance.State,
                        gameInstance.State.RootFaction,
                        gameInstance.Ids.GetNext<Building>(),
                        blueprint,
                        footprint),
                PlaceGameObject(var blueprint, var position, var direction) =>
                    gameInstance => PlopComponentGameObject.Command(gameInstance, blueprint, position, direction),
                _ => throw new ArgumentException($"Unexpected command type: {input}", nameof(input))
            };
        }
    }
}
