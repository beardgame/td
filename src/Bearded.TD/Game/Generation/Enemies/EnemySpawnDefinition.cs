using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Generation.Enemies
{
    class EnemySpawnDefinition
    {
        private readonly double baseProbability;
        private readonly IEnumerable<IEnemySpawnFactor> spawnFactors;

        public string BlueprintId { get; }

        public EnemySpawnDefinition(string blueprintId, double baseProbability)
            : this(blueprintId, baseProbability, Enumerable.Empty<IEnemySpawnFactor>()) { }

        public EnemySpawnDefinition(
            string blueprintId, double baseProbability, IEnumerable<IEnemySpawnFactor> spawnFactors)
        {
            BlueprintId = blueprintId;
            this.baseProbability = baseProbability;
            this.spawnFactors = spawnFactors;
        }

        public double GetProbability(GameInstance game)
        {
            var probability = baseProbability;
            spawnFactors.ForEach(factor => factor.Apply(ref probability, game));
            return probability;
        }
    }
}
