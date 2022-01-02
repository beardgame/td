using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class EnemyPathIndicator : GameObject, ITileWalkerOwner, IRenderable
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

    public EnemyPathIndicator(Tile currentTile)
    {
        startTile = currentTile;
    }

    protected override void OnAdded()
    {
        base.OnAdded();

        tileWalker = new TileWalker(this, Game.Level, startTile);
        passabilityLayer = Game.PassabilityManager.GetLayer(Passability.WalkingUnit);

        var sprite = Game.Meta.Blueprints.Sprites[ModAwareId.ForDefaultMod("particle")].GetSprite("circle-soft");
        drawer = new TrailDrawer(Game, sprite);

        Game.ListAs<IRenderable>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (deleteAt == null)
        {
            tileWalker!.Update(elapsedTime, Constants.Game.Enemy.PathIndicatorSpeed);
        }
        else if (Game.Time > deleteAt)
        {
            Delete();
        }

        var h = Game.GeometryLayer[currentTile].DrawInfo.Height;

        trail.Update(Game.Time, position.WithZ(h + 0.1.U()), deleteAt != null);
    }

    public void OnTileChanged(Tile oldTile, Tile newTile) { }

    public Direction GetNextDirection()
    {
        var desiredDirection = Game.Navigator.GetDirections(currentTile);
        var isPassable = passabilityLayer[currentTile.Neighbor(desiredDirection)].IsPassable;

        if (!isPassable)
            deleteAfterTimeout();

        return isPassable ? desiredDirection : Direction.Unknown;
    }

    private void deleteAfterTimeout()
    {
        deleteAt = Game.Time + trailTimeout;
    }

    public void Render(CoreDrawers drawers)
    {
        drawer.DrawTrail(trail, renderSize, Game.Time, trailTimeout, Color.Orange.WithAlpha());
    }
}
