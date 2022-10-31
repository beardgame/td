using Bearded.TD.Game.Simulation.Projectiles;

namespace Bearded.TD.Game.Simulation.Damage;

enum HitType
{
    Impact,
    AreaOfEffect,
    Self,
}

readonly record struct HitContext(HitType Type, HitInfo? Info);
