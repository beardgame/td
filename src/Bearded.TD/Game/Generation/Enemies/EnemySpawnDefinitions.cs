using System.Collections.Generic;

namespace Bearded.TD.Game.Generation.Enemies
{
    static class EnemySpawnDefinitions
    {
        public static IReadOnlyList<EnemySpawnDefinition> BuildSpawnDefinitions()
        {
            return new []
            {
                new EnemySpawnDefinition("debug", 8),
                new EnemySpawnDefinition("fast", 1),
                new EnemySpawnDefinition("strong", 1),
                new EnemySpawnDefinition("tank", .125),
            };
        }
    }
}
