using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("damageBuildingOnStuck")]
sealed class DamageBuildingOnStuck : Component<DamageBuildingOnStuck.IParameters>, IListener<EnemyGotStuck>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        [Modifiable(10)]
        UntypedDamage Damage { get; }

        DamageType? DamageType { get; }

        IGameObjectBlueprint? SpawnObject { get; }
    }

    public DamageBuildingOnStuck(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(EnemyGotStuck @event)
    {
        var buildings = Owner.Game.BuildingLayer;
        if (!buildings.TryGetMaterializedBuilding(@event.IntendedTarget, out var targetBuilding))
        {
            return;
        }

        var damage = Parameters.Damage.Typed(Parameters.DamageType ?? DamageType.Kinetic);
        var incident = (targetBuilding.Position - Owner.Position).NormalizedSafe();
        var impact = new Impact(targetBuilding.Position, -incident, incident);
        var hit = Hit.FromAreaOfEffect(impact);

        Owner.Sync(DamageGameObject.Command, Owner, targetBuilding, damage, hit);

        if (Parameters.SpawnObject is { } blueprint)
        {
            Owner.Sync(SpawnGameObject.Command, blueprint, Owner, Owner.Position, Owner.Direction);
        }
    }
}
