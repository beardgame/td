using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components.Damage;
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
                    building.GetComponents<IHealth>()
                        .MaybeSingle()
                        .Match(health => building.Damage(
                            new DamageInfo(-health.MaxHealth, DamageType.DivineIntervention, null)));
                }
            }
        }
    }
}
