using System.Linq;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

[Component("damageBuildings")]
sealed class DamageBuildingsInRange : Component<DamageBuildingsInRange.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.Damage)]
        HitPoints Damage { get; }

        [Modifiable(Type = AttributeType.FireRate)]
        Frequency AttackRate { get; }
    }

    private ITilePresence tilePresence = null!;
    private IEnemyMovement? movement;
    private Instant nextAttack;

    public DamageBuildingsInRange(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IEnemyMovement>(Owner, Events, m => movement = m);
    }

    public override void Activate()
    {
        base.Activate();
        tilePresence = Owner.GetTilePresence();
        resetAttackTime();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (movement?.IsMoving ?? true)
        {
            resetAttackTime();
        }
        else
        {
            tryAttack();
        }
    }

    private void resetAttackTime()
    {
        nextAttack = Owner.Game.Time + 1 / Parameters.AttackRate;
    }

    private void tryAttack()
    {
        while (nextAttack <= Owner.Game.Time)
        {
            var desiredDirection = Owner.Game.Navigator.GetDirectionToSink(tilePresence.OccupiedTiles.Single());

            var neighbor = tilePresence.OccupiedTiles.Single().Neighbor(desiredDirection);
            if (!Owner.Game.BuildingLayer.TryGetMaterializedBuilding(neighbor, out var target))
            {
                return;
            }

            var damage = new TypedDamage(Parameters.Damage, DamageType.Kinetic);
            var hit = getHit(neighbor);

            // Experiment: Deal more damage and die

            var secondsOfDamage = 10.S();
            var damageScalar = secondsOfDamage * Parameters.AttackRate;
            damage *= (float)damageScalar;
            if (DamageExecutor.FromObject(Owner).TryDoDamage(target, damage, hit))
            {
                nextAttack += secondsOfDamage;
                Events.Send(new EnactDeath());
            }
            return;

            if (DamageExecutor.FromObject(Owner).TryDoDamage(target, damage, hit))
            {
                nextAttack += 1 / Parameters.AttackRate;
            }
        }
    }

    private Hit getHit(Tile targetTile)
    {
        var target = Level.GetPosition(targetTile).WithZ();
        var position = Owner.Position;

        var rayCaster = new LevelRayCaster();
        rayCaster.StartEnumeratingTiles(new Ray(position.XY(), target.XY()));
        rayCaster.MoveNext();
        rayCaster.MoveNext();

        var direction = new Difference3((target - position).NumericValue.NormalizedSafe());
        var point = position + direction * rayCaster.CurrentRayFactor;

        var impact = new Impact(
            point,
            -direction,
            direction
        );
        return Hit.FromImpact(impact);
    }
}
