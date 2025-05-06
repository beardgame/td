using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

static class Health
{
    public static HitPoints CurrentTotalHitPoints(this GameObject obj) =>
        obj.GetComponents<HitPointsPool>().sumHitPointPools();

    public static HitPoints MaxHitPointsInShell(this GameObject obj, DamageShell damageShell) =>
        obj.GetComponents<HitPointsPool>().Where(pool => pool.Shell == damageShell).sumHitPointPools();

    private static HitPoints sumHitPointPools(this IEnumerable<HitPointsPool> pools) =>
        pools.Aggregate(HitPoints.Zero, (hp, pool) => hp + pool.CurrentHitPoints);
}
