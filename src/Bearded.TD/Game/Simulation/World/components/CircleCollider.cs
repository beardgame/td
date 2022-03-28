using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.World;

sealed class CircleCollider : Component, ICollider
{
    private readonly Unit radius;
    private Circle collisionCircle => new(Owner.Position.XY(), radius);

    public CircleCollider(Unit radius)
    {
        this.radius = radius;
    }

    protected override void OnAdded() {}
    public override void Update(TimeSpan elapsedTime) {}

    public bool TryHit(Ray ray, out float rayFactor, out Position2 point, out Difference2 normal) =>
        collisionCircle.TryHit(ray, out rayFactor, out point, out normal);
}
