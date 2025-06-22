using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;

namespace Bearded.TD.Game.Simulation.Projectiles;

readonly record struct ObjectHit(Hit Hit, GameObject Object) : IComponentEvent;

static class Hits
{
    public static void HitObject(GameObject subject, ComponentEvents events, GameObject obj, Impact impact)
    {
        var potential = subject.GetDamagePotential();

        var hit = Hit.FromImpact(impact, potential);

        events.Send(new ObjectHit(hit, obj));
    }
}
