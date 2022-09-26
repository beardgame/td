using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Simulation.Exploration;

[GameRule("revealRandomZoneOnWaveEnd")]
sealed class RevealRandomZoneOnWaveEnd : GameRule
{
    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(new Listener(context.GameState, context.Dispatcher));
    }

    private sealed class Listener : IListener<WaveEnded>
    {
        private readonly GameState gameState;
        private readonly IDispatcher<GameInstance> dispatcher;

        public Listener(GameState gameState, IDispatcher<GameInstance> dispatcher)
        {
            this.gameState = gameState;
            this.dispatcher = dispatcher;
        }

        public void HandleEvent(WaveEnded @event)
        {
            if (chooseZoneToReveal() is { } zone)
            {
                dispatcher.RunOnlyOnServer(() => RevealZone.Command(gameState, zone));
            }
        }

        private Zone? chooseZoneToReveal()
        {
            if (gameState.ExplorationManager.ExplorableZones.IsDefaultOrEmpty)
            {
                return null;
            }

            var explorableZones = gameState.ExplorationManager.ExplorableZones.ToImmutableHashSet();

            var targetZones = allZonesContainingTargets();
            var seenZones = new HashSet<Zone>(targetZones);
            var front = targetZones;

            while (true)
            {
                var shell = adjacentZonesNotYetSeen(front, seenZones);
                if (shell.IsEmpty)
                {
                    return null;
                }

                var explorableZonesInShell = shell.Where(explorableZones.Contains).ToImmutableArray();
                if (!explorableZonesInShell.IsEmpty)
                {
                    return explorableZonesInShell.RandomElement();
                }

                seenZones.UnionWith(shell);
                front = shell;
            }
        }

        private ImmutableArray<Zone> allZonesContainingTargets()
        {
            return gameState.Enumerate<EnemySink.ITarget>()
                .Select(target => target.Tile)
                .Select(tile => gameState.ZoneLayer.ZoneForTile(tile))
                .NotNull()
                .Distinct()
                .ToImmutableArray();
        }

        private ImmutableArray<Zone> adjacentZonesNotYetSeen(ImmutableArray<Zone> from, ICollection<Zone> seen)
        {
            return from
                .SelectMany(zone => gameState.ZoneLayer.AdjacentZones(zone))
                .WhereNot(seen.Contains)
                .Distinct()
                .ToImmutableArray();
        }
    }
}
