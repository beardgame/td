using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Damage;

static class Health
{
    public static HitPoints CurrentTotalHitPoints(this GameObject obj) => obj.allPools().sumHitPointPools();

    public static HitPoints? CurrentTotalHitPointsOrNull(this GameObject obj) =>
        obj.allPools().sumHitPointPoolsOrNull();

    public static HitPoints MaxHitPointsInShell(this GameObject obj, DamageShell damageShell) =>
        obj.allPoolsInShell(damageShell).sumHitPointPools();

    private static IEnumerable<HitPointsPool> allPoolsInShell(this GameObject obj, DamageShell damageShell) =>
        obj.allPools().Where(pool => pool.Shell == damageShell);

    private static IEnumerable<HitPointsPool> allPools(this GameObject obj) =>
        obj.GetComponents<HitPointsPool>();

    private static HitPoints sumHitPointPools(this IEnumerable<HitPointsPool> pools) =>
        pools.Aggregate(HitPoints.Zero, (hp, pool) => hp + pool.CurrentHitPoints);

    private static HitPoints? sumHitPointPoolsOrNull(this IEnumerable<HitPointsPool> pools) =>
        pools.Aggregate((HitPoints?) null, (hp, pool) => (hp ?? HitPoints.Zero) + pool.CurrentHitPoints);
}
