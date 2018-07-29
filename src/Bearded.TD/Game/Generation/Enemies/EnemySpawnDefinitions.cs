using System.Collections.Generic;

namespace Bearded.TD.Game.Generation.Enemies
{
    static class EnemySpawnDefinitions
    {
        public static IReadOnlyList<EnemySpawnDefinition> BuildSpawnDefinitions()
        {
            return new []
            {
                new EnemySpawnDefinition("standard01", 8),
                new EnemySpawnDefinition("standard02", 8),
                new EnemySpawnDefinition("standard03", 8),
                new EnemySpawnDefinition("fast01", 2),
                new EnemySpawnDefinition("fast02", 2),
                new EnemySpawnDefinition("fast03", 2),
                new EnemySpawnDefinition("strong01", 1),
                new EnemySpawnDefinition("strong02", 1),
                new EnemySpawnDefinition("strong03", 1),
                new EnemySpawnDefinition("tank01", .125),
                new EnemySpawnDefinition("tank02", .125),
            };
        }
    }
}
