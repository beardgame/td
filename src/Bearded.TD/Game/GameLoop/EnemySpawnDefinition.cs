using System.Collections.Generic;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Game.GameLoop;

sealed class EnemySpawnDefinition
{
    public ModAwareId BlueprintId { get; }
    public double Probability { get; }

    private EnemySpawnDefinition(ModAwareId blueprintId, double probability)
    {
        BlueprintId = blueprintId;
        Probability = probability;
    }

    public static IReadOnlyList<EnemySpawnDefinition> All
    {
        get
        {
            return new[]
            {
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("standard01"), 8),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("standard02"), 8),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("standard03"), 8),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("fast01"), 2),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("fast02"), 2),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("fast03"), 2),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("strong01"), 1),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("strong02"), 1),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("strong03"), 1),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("tank01"), .125),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("tank02"), .125),
            };
        }
    }
}