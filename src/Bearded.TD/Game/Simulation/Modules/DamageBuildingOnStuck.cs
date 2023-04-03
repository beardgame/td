using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("damageBuildingOnStuck")]
sealed class DamageBuildingOnStuck : DoSomethingOnStuck<DamageBuildingOnStuck.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        [Modifiable(10)]
        UntypedDamage Damage { get; }

        DamageType? DamageType { get; }

        TimeSpan Delay { get; }

        IGameObjectBlueprint? SpawnObject { get; }
    }

    protected override TimeSpan Delay => Parameters.Delay;

    public DamageBuildingOnStuck(IParameters parameters) : base(parameters) { }

    protected override void DoAction(Tile targetTile)
    {
        var buildings = Owner.Game.BuildingLayer;
        if (!buildings.TryGetMaterializedBuilding(targetTile, out var targetBuilding))
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
