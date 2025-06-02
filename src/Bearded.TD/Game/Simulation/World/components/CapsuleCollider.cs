using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

[Component("capsuleCollider")]
sealed class CapsuleCollider(CapsuleCollider.IParameters parameters)
    : Component<CapsuleCollider.IParameters>(parameters), ICollider, IRadius
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        Unit Radius { get; }
        Unit Height { get; }
        bool Solid { get; }
    }

    public Unit Radius => Parameters.Radius;
    public ColliderType Type => Parameters.Solid ? ColliderType.Solid : ColliderType.Ephemeral;

    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}

    public bool TryHit(Ray3 ray, out float rayFactor, out Position3 point, out Difference3 normal)
    {
        var capsule = new Capsule(
            Owner.Position,
            Owner.Position + new Difference3(0.U(), 0.U(), Parameters.Height),
            Radius + ray.Radius
            );

        var hit = capsule.TryHit(ray, out rayFactor, out point, out normal);

        point -= ray.Radius * normal.NumericValue;

        return hit;
    }
}
