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
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("standard04"), 8),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("standard05"), 8),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("standard06"), 8),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("standard07"), 8),
                new EnemySpawnDefinition(ModAwareId.ForDefaultMod("standard08"), 8),
            };
        }
    }
}
