using Bearded.TD.Commands;
using Bearded.TD.Game.Players;

namespace Bearded.TD.Game.Commands.Debug
{
    static class RevealMap
    {
        public static IRequest<Player, GameInstance> Request(GameInstance game) => Implementation.For(game);

        private sealed class Implementation : UnifiedDebugRequestCommandWithoutParameter<Implementation>
        {
            public override void Execute()
            {
                Game.State.VisibilityLayer.RevealAllZones();
            }
        }
    }
}
