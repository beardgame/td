using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class EnemyPathIndicator : Component, ITileWalkerOwner, IRenderable
{
    private const float renderSize = .1f;
    private static readonly TimeSpan trailTimeout = 1.S();

    private readonly Tile startTile;
    private TileWalker? tileWalker;
    private PassabilityLayer passabilityLayer = null!;
    private readonly TrailTracer trail = new(trailTimeout, newPartDistanceThreshold: 2.U());

    private Position2 position => tileWalker?.Position ?? Level.GetPosition(currentTile);
    private Tile currentTile => tileWalker?.CurrentTile ?? startTile;

    private Instant? deleteAt;
    private TrailDrawer drawer = null!;

    public bool Deleted => Owner.Deleted;

    public EnemyPathIndicator(Tile currentTile)
    {
        startTile = currentTile;
    }

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();

        tileWalker = new TileWalker(this, Owner.Game.Level, startTile);
        passabilityLayer = Owner.Game.PassabilityObserver.GetLayer(Passability.WalkingUnit);

        var sprite = Owner.Game.Meta.Blueprints.Sprites[ModAwareId.ForDefaultMod("particle")].GetSprite("circle-soft");
        drawer = new TrailDrawer(Owner.Game, sprite);

        Owner.Game.ListAs<IRenderable>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (deleteAt == null)
        {
            tileWalker!.Update(elapsedTime, Constants.Game.Enemy.PathIndicatorSpeed);
        }
        else if (Owner.Game.Time > deleteAt)
        {
            Owner.Delete();
        }

        var h = Owner.Game.GeometryLayer[currentTile].DrawInfo.Height;

        trail.Update(Owner.Game.Time, position.WithZ(h + 0.1.U()), deleteAt != null);
    }

    public void OnTileChanged(Tile oldTile, Tile newTile) { }

    public Direction GetNextDirection()
    {
        var desiredDirection = Owner.Game.Navigator.GetDirectionToSink(currentTile);
        var isPassable = passabilityLayer[currentTile.Neighbor(desiredDirection)].IsPassable;

        if (!isPassable)
            deleteAfterTimeout();

        return isPassable ? desiredDirection : Direction.Unknown;
    }

    private void deleteAfterTimeout()
    {
        deleteAt = Owner.Game.Time + trailTimeout;
    }

    public void Render(CoreDrawers drawers)
    {
        drawer.DrawTrail(
            trail, renderSize, Owner.Game.Time, trailTimeout, Constants.Game.GameUI.EnemyIndicatorColor.WithAlpha());
    }
}
