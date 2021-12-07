using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Shared.Events;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Exploration
{
    sealed class ExplorationManager : IListener<ZoneRevealed>
    {
        private readonly GameState gameState;

        private ZoneLayer zoneLayer => gameState.ZoneLayer;
        private VisibilityLayer visibilityLayer => gameState.VisibilityLayer;

        private ImmutableArray<Zone> explorableZones;
        public bool HasExplorationToken { get; private set; }

        public ExplorationManager(GameState gameState)
        {
            this.gameState = gameState;
            gameState.Meta.Events.Subscribe(this);
            recalculateExplorableZones();
        }

        public void HandleEvent(ZoneRevealed @event)
        {
            recalculateExplorableZones();
        }

        private void recalculateExplorableZones()
        {
            explorableZones = zoneLayer.AllZones.Where(
                    zone =>
                        !visibilityLayer[zone].IsRevealed() &&
                        zoneLayer.AdjacentZones(zone).Any(adjZone => visibilityLayer[adjZone].IsRevealed()))
                .ToImmutableArray();
            gameState.Meta.Events.Send(new ExplorableZonesChanged(explorableZones));
        }

        public void ConsumeExplorationToken()
        {
            State.Satisfies(HasExplorationToken);
            HasExplorationToken = false;
        }

        public void AwardExplorationToken()
        {
            HasExplorationToken = true;
        }
    }
}
