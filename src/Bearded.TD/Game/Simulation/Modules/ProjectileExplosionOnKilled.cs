using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("projectileExplosionOnKilled")]
sealed class ProjectileExplosionOnKilled : Component<ProjectileExplosionOnKilled.IParameters>, IListener<ObjectKilled>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Projectile { get; }

        [Modifiable(7)]
        int MinProjectileNumber { get; }
        [Modifiable(10)]
        int MaxProjectileNumber { get; }

        [Modifiable(1)]
        Speed RandomVelocity { get; }

        [Modifiable(30)]
        UntypedDamage Damage { get; }
    }

    public ProjectileExplosionOnKilled(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(ObjectKilled @event)
    {
        var projectiles = ProjectileExplosion.CreateProjectilesForExplosion(
            Parameters.Projectile,
            Owner,
            Parameters.Damage,
            Parameters.MinProjectileNumber,
            Parameters.MaxProjectileNumber,
            Parameters.RandomVelocity);
        foreach (var projectile in projectiles)
        {
            Owner.Game.Add(projectile);
        }
    }
}
