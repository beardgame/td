using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

[Component("shockwave")]
sealed class Shockwave : Component<Shockwave.IParameters>
{
    private readonly HashSet<GameObject> seen = new();
    private DynamicDamage damageProvider = null!;

    private Unit range;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        Unit MinRange { get; }
        Unit MaxRange { get; }
        Speed Speed { get; }
        UntypedDamage MinRangeDamage { get; }
        UntypedDamage MaxRangeDamage { get; }
    }

    public Shockwave(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        damageProvider = new DynamicDamage();
        Owner.AddComponent(damageProvider);
    }

    public override void Activate()
    {
        range = Parameters.MinRange;
    }

    public override void Update(TimeSpan elapsedTime)
    {
        range += Parameters.Speed * elapsedTime;
        if (range > Parameters.MaxRange)
            range = Parameters.MaxRange;

        updateDamage();

        var tiles = Level.TilesWithCenterInCircle(Owner.Position.XY(), range);
        var newTargets = tiles
            .SelectMany(objectsOnTile)
            .Where(o => !seen.Contains(o))
            .Where(inRange)
            .Where(seen.Add);

        foreach (var target in newTargets)
        {
            tryHit(target);
        }

        if (range == Parameters.MaxRange)
            Owner.RemoveComponent(this);
    }

    private void updateDamage()
    {
        var damageFactor = (range - Parameters.MinRange) / (Parameters.MaxRange - Parameters.MinRange);
        var damage = Parameters.MinRangeDamage * (1 - damageFactor) + Parameters.MaxRangeDamage * damageFactor;
        damageProvider.Inject(damage);
    }

    private IEnumerable<GameObject> objectsOnTile(Tile tile)
    {
        var objects = Owner.Game.PhysicsLayer.GetObjectsOnTile(tile);

        if (Owner.Game.BuildingLayer.TryGetMaterializedBuilding(tile, out var building))
            return objects.Append(building);

        return objects;
    }

    private bool inRange(GameObject obj)
    {
        return (obj.Position - Owner.Position).LengthSquared < range.Squared;
    }

    private void tryHit(GameObject obj)
    {
        var direction = Owner.Position - obj.Position;
        var impact = new Impact(obj.Position, -direction, direction);
        Events.Send(new TouchObject(obj, impact));
    }
}
