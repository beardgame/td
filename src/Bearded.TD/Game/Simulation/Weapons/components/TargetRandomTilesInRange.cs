using System;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("targetRandomTilesInRange")]
sealed class TargetRandomTilesInRange(TargetRandomTilesInRange.IParameters parameters)
    : Component<TargetRandomTilesInRange.IParameters>(parameters),
        ITargeter<IPositionable>,
        IWeaponAimer,
        IWeaponTrigger,
        IPositionable
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        TimeSpan RetargetInterval { get; }

        Unit? MaxRetargetDistance { get; }

        [Modifiable(0)]
        Speed RandomDrift { get; }
    }

    public IPositionable Target => this;

    public Position3 Position { get; private set; }

    public Direction2 AimDirection => (Position - Owner.Position).XY().Direction;

    public bool TriggerPulled => true;

    private IWeaponRange? range;
    private Velocity3 drift;
    private Instant nextRetarget;

    public override void Activate()
    {
        ComponentDependencies.Depend<IWeaponRange>(Owner, Events, r => range = r);

        if (Owner.TryGetSingleComponent<IWeaponState>(out var weapon))
        {
            if (range == null)
            {
                Position = Owner.Position + (weapon.Direction * 1.5.U()).WithZ();
            }
            else
            {
                var tile = range.GetTilesInRange()
                    .Select(t => (Tile: t, Vector: Level.GetPosition(t) - Owner.Position.XY()))
                    .Where(t => (Direction2.Of(t.Vector.NumericValue) - weapon.Direction).MagnitudeInDegrees < 25)
                    .MaxBy(t => t.Vector.LengthSquared)
                    .Tile;
                Position = Level.GetPosition(tile).WithZ(0.25f);
            }
        }

        targetNewTile();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        Position += drift * elapsedTime;

        if (nextRetarget <= Owner.Game.Time)
            targetNewTile();
    }

    private void targetNewTile()
    {
        nextRetarget = Owner.Game.Time + Parameters.RetargetInterval;

        if (range == null)
            return;

        var tiles = range.GetTilesInRange();
        if (tiles.Length == 0)
            return;

        var minTileDistanceSquared = (tiles.Max(
            t => (Level.GetPosition(t) - Owner.Position.XY()).Length.NumericValue)
            * 0.5.U()).Squared;

        var maxDSquared = Parameters.MaxRetargetDistance?.Squared
            ?? Squared<Unit>.FromValue(float.MaxValue);

        var tilesInRange = tiles
            .Where(t =>
            {
                var p = Level.GetPosition(t);
                return (p - Position.XY()).LengthSquared <= maxDSquared
                    && (p - Owner.Position.XY()).LengthSquared >= minTileDistanceSquared;
            })
            .ToList();

        var tile = tilesInRange.Count > 0
            ? tilesInRange.RandomElement()
            : tiles.RandomElement();

        Position = Level.GetPosition(tile).WithZ(0.25f);

        drift = (Parameters.RandomDrift * Direction2.FromDegrees(Random.Shared.NextFloat(0, 360))).WithZ();
    }

}
