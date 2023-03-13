using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Exploration;

[Component("revealSurroundingZones")]
sealed class RevealSurroundingZones : Component<RevealSurroundingZones.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        public int Steps { get; }
    }

    private ITilePresenceListener? tilePresenceListener;

    public RevealSurroundingZones(IParameters parameters) : base(parameters) {}

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();
        tilePresenceListener = Owner.GetTilePresence().ObserveAdditions(onTileAdded);
    }

    public override void OnRemoved()
    {
        tilePresenceListener?.Detach();
        base.OnRemoved();
    }

    private void onTileAdded(Tile t)
    {
        if (Owner.Game.ZoneLayer.ZoneForTile(t) is { } zone)
        {
            recursivelyRevealZones(zone, Parameters.Steps);
        }
    }

    private void recursivelyRevealZones(Zone z, int steps)
    {
        if (!Owner.Game.VisibilityLayer.RevealZone(z) || steps <= 0)
        {
            return;
        }

        foreach (var adjacentZone in Owner.Game.ZoneLayer.AdjacentZones(z))
        {
            recursivelyRevealZones(adjacentZone, steps - 1);
        }
    }

    public override void Update(TimeSpan elapsedTime) {}
}
