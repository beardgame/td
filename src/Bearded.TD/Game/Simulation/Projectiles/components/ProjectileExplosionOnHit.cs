using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("projectileExplosionOnHit")]
sealed class ProjectileExplosionOnHit
    : Component<ProjectileExplosionOnHit.IParameters>, IListener<HitLevel>, IListener<HitEnemy>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Projectile { get; }

        bool OnHitLevel { get; }

        bool OnHitEnemy { get; }

        [Modifiable(7)]
        int MinProjectileNumber { get; }
        [Modifiable(10)]
        int MaxProjectileNumber { get; }

        [Modifiable(1)]
        Speed RandomVelocity { get; }

        [Modifiable(1)]
        double DamageFactor { get; }
    }

    public ProjectileExplosionOnHit(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Subscribe<HitEnemy>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Subscribe<HitLevel>(this);
        }
    }

    public override void OnRemoved()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Unsubscribe<HitEnemy>(this);
        }

        if (Parameters.OnHitLevel)
        {
            Events.Unsubscribe<HitLevel>(this);
        }
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(HitLevel e)
    {
        onHit(e.Info);
    }

    public void HandleEvent(HitEnemy e)
    {
        onHit(e.Impact);
    }

    private void onHit(Impact hit)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var parentDamage))
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        var projectileNumber = getProjectileNumber(parentDamage);

        var projectileDamage = parentDamage / projectileNumber;

        foreach (var _ in Enumerable.Range(0, projectileNumber))
        {
            var velocity = Vectors.GetRandomUnitVector3() * Parameters.RandomVelocity;
            var direction = Direction2.Of(velocity.NumericValue.Xy);

            var projectile = ProjectileFactory
                .Create(Parameters.Projectile, Owner, Owner.Position, direction, velocity, projectileDamage, default);
            projectile.AddComponent(new Property<Impact>(hit));
            Owner.Game.Add(projectile);
        }
    }

    private int getProjectileNumber(UntypedDamage parentDamage)
    {
        var desiredDamage = (int) (parentDamage.Amount.NumericValue * Parameters.DamageFactor);

        if (desiredDamage <= Parameters.MaxProjectileNumber)
        {
            return desiredDamage;
        }

        var bestProjectileNumber = Parameters.MaxProjectileNumber;
        var bestTotalDamage = desiredDamage / bestProjectileNumber * bestProjectileNumber;

        for (var n = Parameters.MinProjectileNumber; n < Parameters.MaxProjectileNumber; n++)
        {
            var totalDamage = desiredDamage / n * n;
            if (totalDamage > bestTotalDamage)
            {
                bestProjectileNumber = n;
                bestTotalDamage = totalDamage;
            }
        }

        return bestProjectileNumber;
    }
}
