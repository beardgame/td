using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Exploration;

namespace Bearded.TD.Game.Commands.Debug;

static class RevealMap
{
    public static IRequest<Player, GameInstance> Request(GameInstance game) => Implementation.For(game);

    private sealed class Implementation : UnifiedDebugRequestCommandWithoutParameter<Implementation>
    {
        public override void Execute()
        {
            if (Game.Me.Faction.TryGetBehaviorIncludingAncestors<FactionVisibility>(out var visibility))
            {
                visibility.RevealAllZones();
                return;
            }
            Game.Meta.Logger.Warning?.Log("Cannot reveal visibility if your faction does not track visibility.");
        }
    }
}
