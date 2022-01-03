using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("targetEnemiesInRange")]
sealed class TargetEnemiesInRange
    : Component<ComponentGameObject, ITargetEnemiesInRange>,
        IWeaponRangeDrawer,
        ITargeter<IPositionable>,
        IWeaponAimer,
        IWeaponTrigger,
        IWeaponRange,
        IListener<DrawComponents>
{
    private PassabilityLayer passabilityLayer = null!;
    private TileRangeDrawer tileRangeDrawer = null!;
    private IWeaponState weapon = null!;

    // mutable state
    private Instant endOfIdleTime;
    private Instant nextTileInRangeRecalculationTime;

    private Angle? currentMaxTurningAngle;
    private Unit currentRange;
    private ImmutableArray<Tile> tilesInRange = ImmutableArray<Tile>.Empty;

    private ComponentGameObject? target;
    public IPositionable? Target => target;

    private bool dontDrawThisFrame;

    private GameState game => Owner.Game;
    private Position3 position => Owner.Position;

    public bool TriggerPulled { get; private set; }
    public Direction2 AimDirection { get; private set; }

    public Unit Range => currentRange;

    public TargetEnemiesInRange(ITargetEnemiesInRange parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c => weapon = c);
        passabilityLayer = game.PassabilityManager.GetLayer(Passability.Projectile);
        tileRangeDrawer = new TileRangeDrawer(
            game, () => weapon.RangeDrawStyle, getTilesToDraw, Color.Green);

        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        tryShootingAtTarget();
    }

    private void tryShootingAtTarget()
    {
        if (!weapon.IsEnabled)
        {
            turnOff();
            return;
        }

        if (endOfIdleTime > game.Time)
            return;

        ensureTilesInRangeList();
        ensureTargetingState();

        if (target == null)
        {
            goIdle();
            return;
        }

        shootAt(target.Position);
    }

    private void shootAt(Position3 targetPosition)
    {
        AimDirection = (targetPosition - position).XY().Direction;
        TriggerPulled = true;
    }

    private void goIdle()
    {
        if(weapon.MaximumTurningAngle != null)
            AimDirection = weapon.NeutralDirection;
        TriggerPulled = false;

        endOfIdleTime = game.Time + Parameters.NoTargetIdleInterval;
    }

    private void turnOff()
    {
        TriggerPulled = false;
    }

    private void ensureTilesInRangeList()
    {
        if (currentRange == Parameters.Range
            && currentMaxTurningAngle.Equals(weapon.MaximumTurningAngle)
            && nextTileInRangeRecalculationTime > game.Time)
            return;

        recalculateTilesInRange();
    }

    private void recalculateTilesInRange()
    {
        currentMaxTurningAngle = weapon.MaximumTurningAngle;
        currentRange = Parameters.Range;
        var rangeSquared = currentRange.Squared;

        var level = game.Level;
        var navigator = game.Navigator;

        var visibilityChecker = currentMaxTurningAngle is { } maxAngle
            ? new LevelVisibilityChecker().InDirection(weapon.NeutralDirection, maxAngle)
            : new LevelVisibilityChecker();

        tilesInRange = visibilityChecker.EnumerateVisibleTiles(
                level,
                position.XY(),
                currentRange,
                t => !level.IsValid(t) || !passabilityLayer[t].IsPassable)
            .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                (Level.GetPosition(t.tile) - position.XY()).LengthSquared < rangeSquared)
            .Select(t => t.tile)
            .OrderBy(navigator.GetDistanceToClosestSink)
            .ToImmutableArray();

        nextTileInRangeRecalculationTime = game.Time + Parameters.ReCalculateTilesInRangeInterval;
    }

    private void ensureTargetingState()
    {
        if (target?.Deleted == true)
            target = null;

        // TODO: accumulating tiles each frame is expensive, can we somehow cache this?
        if (target != null && !tilesInRange.OverlapsWithTiles(OccupiedTileAccumulator.AccumulateOccupiedTiles(target)))
            target = null;

        if (target != null)
            return;

        tryFindTarget();
    }

    private void tryFindTarget()
    {
        target = tilesInRange
            .SelectMany(game.UnitLayer.GetUnitsOnTile)
            .FirstOrDefault();
    }

    public void HandleEvent(DrawComponents e)
    {
        tileRangeDrawer.Draw();
    }

    private IEnumerable<Tile> getTilesToDraw()
    {
        if (dontDrawThisFrame)
        {
            dontDrawThisFrame = false;
            return ImmutableHashSet<Tile>.Empty;
        }

        var allTiles = ImmutableHashSet.CreateRange(Owner.GetComponents<ITurret>()
            .Select(t => (IComponentOwner) t.Weapon)
            .SelectMany(w => w.GetComponents<IWeaponRangeDrawer>())
            .SelectMany(ranger => ranger.TakeOverDrawingThisFrame()));
        dontDrawThisFrame = false;
        return allTiles;
    }

    IEnumerable<Tile> IWeaponRangeDrawer.TakeOverDrawingThisFrame()
    {
        dontDrawThisFrame = true;
        recalculateTilesInRange();
        return tilesInRange;
    }
}
