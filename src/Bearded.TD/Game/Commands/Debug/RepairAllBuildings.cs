using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.GameState.Buildings;
using Bearded.TD.Game.GameState.Components.Damage;
using Bearded.TD.Game.GameState.Damage;
using Bearded.TD.Game.Players;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands.Debug
{
    static class RepairAllBuildings
    {
        public static IRequest<Player, GameInstance> Request(GameInstance game)
            => Implementation.For(game);

        private class Implementation : UnifiedDebugRequestCommandWithoutParameter<Implementation>
        {
            public override void Execute()
            {
                foreach (var building in Game.State.GameObjects.OfType<Building>())
                {
                    building.GetComponents<Health<Building>>()
                        .MaybeSingle()
                        .Match(health => building.Damage(
                            new DamageInfo(
                                -health.MaxHealth,
                                DamageType.DivineIntervention,
                                new DummyDamageOwner(Game.State.RootFaction)
                                )
                            ));
                }
            }
        }
    }
}
