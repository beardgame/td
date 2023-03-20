using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

static class AreaOfEffect
{
    public static void Damage(
        GameState game, DamageExecutor damageExecutor, TypedDamage damage, Position3 center, Unit range)
    {
        foreach (var (obj, impact) in findObjects(game, center, range))
        {
            damageExecutor.TryDoDamage(obj, damage, Hit.FromAreaOfEffect(impact));
        }
    }

    public static void ApplyStatusEffect<T>(
        GameState game, T effect, Position3 center, Unit range, float? probability = null, Random? random = null)
        where T : IElementalEffect
    {
        random ??= Random.Shared;

        foreach (var (obj, _) in findObjects(game, center, range))
        {
            if (probability is { } p && !random.NextBool(p)) continue;
            obj.TryApplyEffect(effect);
        }
    }

    private static IEnumerable<ObjectInRange> findObjects(GameState game, Position3 center, Unit range)
    {
        var objects = game.PhysicsLayer;
        var rangeSquared = range.Squared;

        // Returns only tiles with their centre in the circle with the given range.
        // This means it may miss enemies that are strictly speaking in range, but are on a tile that itself is out
        // of range.
        var tiles = Level.TilesWithCenterInCircle(center.XY(), range);

        foreach (var obj in tiles.SelectMany(objects.GetObjectsOnTile))
        {
            var difference = obj.Position - center;

            if (difference.LengthSquared > rangeSquared)
                continue;

            var incident = new Difference3(difference.NumericValue.NormalizedSafe());
            var impact = new Impact(obj.Position, -incident, incident);

            yield return new ObjectInRange(obj, impact);
        }
    }

    private readonly record struct ObjectInRange(GameObject GameObject, Impact Impact);
}
