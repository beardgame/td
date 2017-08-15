using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Generation.Enemies
{
    class EnemySpawnDefinition
    {
        private readonly double baseProbability;
        private readonly IEnumerable<IEnemySpawnFactor> spawnFactors;

        public UnitBlueprint Blueprint { get; }

        public EnemySpawnDefinition(UnitBlueprint blueprint, double baseProbability)
            : this(blueprint, baseProbability, Enumerable.Empty<IEnemySpawnFactor>()) { }

        public EnemySpawnDefinition(
            UnitBlueprint blueprint, double baseProbability, IEnumerable<IEnemySpawnFactor> spawnFactors)
        {
            Blueprint = blueprint;
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
