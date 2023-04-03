using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("projectileExplosionOnHit")]
sealed class ProjectileExplosionOnHit
    : Component<ProjectileExplosionOnHit.IParameters>, IListener<HitLevel>, IListener<HitObject>
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
        float DamageFactor { get; }
    }

    public ProjectileExplosionOnHit(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        if (Parameters.OnHitEnemy)
        {
            Events.Subscribe<HitObject>(this);
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
            Events.Unsubscribe<HitObject>(this);
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

    public void HandleEvent(HitObject e)
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

        var projectiles = ProjectileExplosion.CreateProjectilesForExplosion(
            Parameters.Projectile,
            Owner,
            parentDamage * Parameters.DamageFactor,
            Parameters.MinProjectileNumber,
            Parameters.MaxProjectileNumber,
            Parameters.RandomVelocity);
        foreach (var projectile in projectiles)
        {
            projectile.AddComponent(new Property<Impact>(hit));
            Owner.Game.Add(projectile);
        }
    }
}
