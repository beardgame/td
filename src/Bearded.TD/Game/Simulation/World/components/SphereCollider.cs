using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

sealed class SphereCollider : Component, ICollider, IRadius
{
    public Unit Radius { get; }
    private Sphere collisionSphere => new(Owner.Position, Radius);

    public SphereCollider(Unit radius)
    {
        Radius = radius;
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
