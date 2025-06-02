using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

[Component("sphereCollider")]
sealed class SphereCollider(SphereCollider.IParameters parameters)
    : Component<SphereCollider.IParameters>(parameters), ICollider, IRadius
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        Unit Radius { get; }
        bool Solid { get; }
    }

    public Unit Radius => Parameters.Radius;
    public ColliderType Type => Parameters.Solid ? ColliderType.Solid : ColliderType.Ephemeral;

    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}

    public bool TryHit(Ray3 ray, out float rayFactor, out Position3 point, out Difference3 normal)
    {
        var hit = new Sphere(Owner.Position, Parameters.Radius + ray.Radius)
            .TryHit(ray, out rayFactor, out point, out normal);

        point -= ray.Radius * normal.NumericValue;

        return hit;
    }
}

interface IRadius
{
    Unit Radius { get; }
}
