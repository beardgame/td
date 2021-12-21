using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Shared.Events;
using static Bearded.TD.Utilities.DebugAssert;
using Faction = Bearded.TD.Game.Simulation.Factions.Faction;

namespace Bearded.TD.Game.Simulation.Exploration;

[FactionBehavior("exploration")]
sealed class FactionExploration : FactionBehavior<Faction>, IListener<ZoneRevealed>
{
    // private readonly GameState gameState;

    private ZoneLayer zoneLayer => gameState.ZoneLayer;
    private FactionVisibility factionVisibility => gameState.VisibilityLayer;

    public ImmutableArray<Zone> ExplorableZones { get; private set; }
    public bool HasExplorationToken { get; private set; }

    public FactionExploration(GameState gameState)
    {
        this.gameState = gameState;
    }

    protected override void Execute()
    {
        Events.Subscribe(this);
        recalculateExplorableZones();
    }

    public void HandleEvent(ZoneRevealed @event)
    {
        recalculateExplorableZones();
    }

    private void recalculateExplorableZones()
    {
        ExplorableZones = zoneLayer.AllZones.Where(
                zone =>
                    !factionVisibility[zone].IsRevealed() &&
                    zoneLayer.AdjacentZones(zone).Any(adjZone => factionVisibility[adjZone].IsRevealed()))
            .ToImmutableArray();
        Events.Send(new ExplorableZonesChanged(Owner, ExplorableZones));
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
