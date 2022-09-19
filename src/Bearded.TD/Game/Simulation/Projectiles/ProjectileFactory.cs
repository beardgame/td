using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

readonly record struct OptionalProjectileProperties(IPositionable? TargetPosition, GameObject? Target);

readonly record struct Target(GameObject Object);
readonly record struct TargetPosition(IPositionable Target);

static class ProjectilePropertyExtensions
{
    public static Target AsTarget(this GameObject obj) => new(obj);
    public static TargetPosition AsTargetPosition(this IPositionable obj) => new(obj);
}

static class ProjectileFactory
{
    public static GameObject Create(
        IGameObjectBlueprint blueprint,
        GameObject parent,
        Position3 position,
        Direction2 direction,
        Velocity3 muzzleVelocity,
        UntypedDamage damage,
        OptionalProjectileProperties properties)
    {
        var obj = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, parent, position, direction);

        obj.AddComponent(new ParabolicMovement(muzzleVelocity));
        obj.AddComponent(new PointCollider());
        obj.AddComponent(new Property<UntypedDamage>(damage));

        if (properties.Target is { } target)
            obj.AddComponent(Property.From(target.AsTarget()));

        if (properties.TargetPosition is { } targetPosition)
            obj.AddComponent(Property.From(targetPosition.AsTargetPosition()));

        return obj;
    }
}
