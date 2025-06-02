using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Physics;

readonly record struct CollidingWithObject(GameObject GameObject, Impact Impact, bool Collided)
    : IComponentPreviewEvent;

readonly record struct ObjectEnteredWithoutHit(GameObject GameObject, Impact Impact) : IComponentEvent;

readonly record struct ColliderType(bool CanBeHit)
{
    public static ColliderType Solid => new(true);
    public static ColliderType Ephemeral => new(false);
}

static class Collision
{
    public static void TouchObject(
        GameObject subject, ComponentEvents events,
        Position3 point, Difference3 step, GameObject obj, Difference3 normal,
        out bool abortCollisionChecksForThisFrame)
    {
        abortCollisionChecksForThisFrame = false;
        var impact = new Impact(point, normal, step.NormalizedSafe());

        if (!obj.TryGetSingleComponent<ICollider>(out var collider))
        {
            DebugAssert.State.IsInvalid("Touched object should have collider.");
            return;
        }

        var colliderType = collider.Type;

        if (!colliderType.CanBeHit)
        {
            events.Send(new ObjectEnteredWithoutHit(obj, impact));
            return;
        }

        // TODO: damage potential needs to be changed somewhere in or right after this call
        // - in damage executor?
        Hits.HitObject(subject, events, obj, impact);

        // TODO: then there needs to be an after-hit event
        // and a DeleteAfterHitIfZeroDamagePotential component (naming) - can probably injected into all projectiles

        if (subject.TryGetProperty(out UntypedDamage damagePotential) && damagePotential <= UntypedDamage.Zero)
        {
            subject.Delete();
            abortCollisionChecksForThisFrame = true;
            return;
        }

        // TODO: all bullet projectiles should have ElasticCollision components so they respond to ricochet
        // check all other components for how they want to handle this
        // and make fire particles use this new behaviour (i.e. not lose their damage potential above)

        var collision = new CollidingWithObject(obj, impact, false);

        events.Preview(ref collision);

        if (collision.Collided)
        {
            abortCollisionChecksForThisFrame = true;
            events.Send(new CollidedWithObject(obj, impact));
        }
    }

    public static void TouchLevel(
        ComponentEvents events,
        Position3 point, Difference3 step, Direction? withStep, Tile tile)
    {
        var normal = new Difference3(withStep?.Vector().WithZ() ?? Vector3.UnitZ);
        var info = new Impact(point, normal, step.NormalizedSafe());
        events.Send(new CollidedWithLevel(info, tile));
    }
}

