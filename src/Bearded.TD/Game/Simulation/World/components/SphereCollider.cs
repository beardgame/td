using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

sealed class SphereCollider : Component, ICollider
{
    private readonly Unit radius;
    private Sphere collisionSphere => new(Owner.Position, radius);

    public SphereCollider(Unit radius)
    {
        this.radius = radius;
    }

    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}

    public bool TryHit(Ray3 ray, out float rayFactor, out Position3 point, out Difference3 normal) =>
        collisionSphere.TryHit(ray, out rayFactor, out point, out normal);
}
