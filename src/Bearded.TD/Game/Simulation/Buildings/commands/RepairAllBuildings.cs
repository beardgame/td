using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

static class RepairAllBuildings
{
    public static IRequest<Player, GameInstance> Request(GameInstance game)
        => Implementation.For(game);

    private sealed class Implementation : UnifiedDebugRequestCommandWithoutParameter<Implementation>
    {
        public override void Execute()
        {
            foreach (var building in Game.State.GameObjects.OfType<GameObject>())
            {
                if (!building.TryGetSingleComponent<IBuildingState>(out _)
                    || !building.TryGetSingleComponent<IHealthEventReceiver>(out var healthEventReceiver))
                {
                    continue;
                }
                healthEventReceiver.Heal(new HealInfo(HitPoints.Max));
            }
        }
    }
}
