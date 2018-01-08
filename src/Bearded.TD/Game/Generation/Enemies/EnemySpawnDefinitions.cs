using System.Collections.Generic;

namespace Bearded.TD.Game.Generation.Enemies
{
    static class EnemySpawnDefinitions
    {
        public static IReadOnlyList<EnemySpawnDefinition> BuildSpawnDefinitions()
        {
            return new []
            {
                new EnemySpawnDefinition("debug0", 8),
                new EnemySpawnDefinition("fast0", 1),
                new EnemySpawnDefinition("strong0", 1),
                new EnemySpawnDefinition("tank0", .125),
            };
        }
    }
}
