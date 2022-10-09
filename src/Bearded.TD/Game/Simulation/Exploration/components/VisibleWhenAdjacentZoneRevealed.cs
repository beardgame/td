using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

[Component("visibleWhenAdjacentZoneRevealed")]
sealed class VisibleWhenAdjacentZoneRevealed : Component, IVisibility, IListener<ZoneRevealed>
{
    private ImmutableHashSet<Zone> adjacentZones = ImmutableHashSet<Zone>.Empty;

    public ObjectVisibility Visibility { get; private set; } = ObjectVisibility.Invisible;

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();

        if (Owner.Game.ZoneLayer.ZoneForTile(Level.GetTile(Owner.Position)) is not { } zone)
        {
            return;
        }

        adjacentZones = Owner.Game.ZoneLayer.AdjacentZones(zone).ToImmutableHashSet();
        if (Owner.Game.ZoneLayer.AdjacentZones(zone).Any(z => Owner.Game.VisibilityLayer[z].IsRevealed()))
        {
            Visibility = ObjectVisibility.Visible;
        }

        Owner.Game.Meta.Events.Subscribe(this);
    }

    public void HandleEvent(ZoneRevealed @event)
    {
        if (adjacentZones.Contains(@event.Zone))
        {
            Visibility = ObjectVisibility.Visible;
        }
    }

    public override void Update(TimeSpan elapsedTime) {}
}
