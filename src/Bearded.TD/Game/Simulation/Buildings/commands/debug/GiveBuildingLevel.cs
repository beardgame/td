using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

static class GiveBuildingLevel
{
    public static IRequest<Player, GameInstance> Request(GameInstance game)
        => Implementation.For(game);

    private sealed class Implementation : UnifiedDebugRequestCommandWithoutParameter<Implementation>
    {
        public override void Execute()
        {
            foreach (var building in Game.State.GameObjects)
            {
                if (building.TryGetSingleComponent<Veterancy.Veterancy>(out var veterancy))
                {
                    veterancy.ForceLevelUp();
                }
            }
        }
    }
}
