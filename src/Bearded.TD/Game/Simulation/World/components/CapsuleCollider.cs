using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

[Component("capsuleCollider")]
sealed class CapsuleCollider : Component<CapsuleCollider.IParameters>, ICollider, IRadius
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        Unit Radius { get; }
        Unit Height { get; }
        bool Solid { get; }
    }

    public Unit Radius => Parameters.Radius;
    public bool IsSolid => Parameters.Solid;

    private Capsule capsule => new(Owner.Position, Owner.Position + new Difference3(0.U(), 0.U(), Parameters.Height), Radius);

    public CapsuleCollider(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}

    public bool TryHit(Ray3 ray, out float rayFactor, out Position3 point, out Difference3 normal) =>
        capsule.TryHit(ray, out rayFactor, out point, out normal);

}
