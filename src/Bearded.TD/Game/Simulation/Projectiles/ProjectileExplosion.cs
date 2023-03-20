using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

static class ProjectileExplosion
{
    public static IEnumerable<GameObject> CreateProjectilesForExplosion(
        IGameObjectBlueprint blueprint,
        GameObject owner,
        UntypedDamage damage,
        int minProjectileCount,
        int maxProjectileCount,
        Speed randomVelocity)
    {
        var projectileNumber = getProjectileNumber(damage, minProjectileCount, maxProjectileCount);

        var projectileDamage = damage / projectileNumber;

        foreach (var _ in Enumerable.Range(0, projectileNumber))
        {
            var velocity = Vectors.GetRandomUnitVector3() * randomVelocity;
            var direction = Direction2.Of(velocity.NumericValue.Xy);

            var projectile = ProjectileFactory
                .Create(blueprint, owner, owner.Position, direction, velocity, projectileDamage, default);
            yield return projectile;
        }
    }

    private static int getProjectileNumber(UntypedDamage damage, int minProjectileCount, int maxProjectileCount)
    {
        var desiredDamage = (int) damage.Amount.NumericValue;

        if (desiredDamage <= maxProjectileCount)
        {
            return desiredDamage;
        }

        var bestProjectileNumber = maxProjectileCount;
        var bestTotalDamage = desiredDamage / bestProjectileNumber * bestProjectileNumber;

        for (var n = minProjectileCount; n < maxProjectileCount; n++)
        {
            var totalDamage = desiredDamage / n * n;
            if (totalDamage > bestTotalDamage)
            {
                bestProjectileNumber = n;
                bestTotalDamage = totalDamage;
            }
        }

        return bestProjectileNumber;
    }
}
