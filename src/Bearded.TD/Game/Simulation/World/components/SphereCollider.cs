using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

[Component("sphereCollider")]
sealed class SphereCollider : Component<SphereCollider.IParameters>, ICollider, IRadius
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        Unit Radius { get; }
        bool Solid { get; }
    }

    public Unit Radius => Parameters.Radius;
    public bool IsSolid => Parameters.Solid;

    private Sphere collisionSphere => new(Owner.Position, Parameters.Radius);

    public SphereCollider(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}

    public bool TryHit(Ray3 ray, out float rayFactor, out Position3 point, out Difference3 normal) =>
        collisionSphere.TryHit(ray, out rayFactor, out point, out normal);

}

interface IRadius
{
    Unit Radius { get; }
}
