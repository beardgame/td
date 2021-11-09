using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands.Debug
{
    static class RepairAllBuildings
    {
        public static IRequest<Player, GameInstance> Request(GameInstance game)
            => Implementation.For(game);

        private sealed class Implementation : UnifiedDebugRequestCommandWithoutParameter<Implementation>
        {
            public override void Execute()
            {
                foreach (var building in Game.State.GameObjects.OfType<Building>())
                {
                    if (!building.TryGetSingleComponent<IHealthEventReceiver>(out var healthEventReceiver))
                    {
                        continue;
                    }
                    healthEventReceiver.Heal(new HealInfo(HitPoints.Max));
                }
            }
        }
    }
}
