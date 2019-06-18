using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Players;

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
                    if (building.GetComponent<Health<Building>>() is var healthComponent)
                    {
                        building.Damage(-healthComponent.MaxHealth);
                    }
                }
            }
        }
    }
}
