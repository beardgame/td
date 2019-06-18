using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Units;

namespace Bearded.TD.Game.Commands.Debug
{
    static class KillAllEnemies
    {
        public static IRequest<Player, GameInstance> Request(GameInstance game)
            => Implementation.For(game);

        private class Implementation : UnifiedDebugRequestCommandWithoutParameter<Implementation>
        {
            public override void Execute()
            {
                foreach (var enemy in Game.State.GameObjects.OfType<EnemyUnit>())
                {
                    enemy.Execute();
                }
            }
        }
    }
}
