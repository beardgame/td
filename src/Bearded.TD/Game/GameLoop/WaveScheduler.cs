using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands.GameLoop;
using Bearded.TD.Game.GameState.GameLoop;
using Bearded.TD.Game.GameState.Resources;
using Bearded.TD.Game.GameState.Units;
using Bearded.TD.Game.Generation.Enemies;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop
{
    sealed class WaveScheduler
    {
        private readonly Random random = new();
        private readonly GameInstance game;
        private readonly ICommandDispatcher<GameInstance> commandDispatcher;
        public event VoidEventHandler? WaveEnded;

        public WaveScheduler(GameInstance game, ICommandDispatcher<GameInstance> commandDispatcher)
        {
            this.game = game;
            this.commandDispatcher = commandDispatcher;
        }

        public void StartWave(WaveRequirements requirements)
        {
            var dormantSpawnLocations = game.State.Enumerate<SpawnLocation>().Where(s => !s.IsAwake).ToList();
            if (dormantSpawnLocations.Count > 0)
            {
                commandDispatcher.Dispatch(WakeUpSpawnLocation.Command(dormantSpawnLocations.RandomElement()));
            }

            commandDispatcher.Dispatch(ExecuteWaveScript.Command(game, createWaveScript(requirements)));
        }

        private WaveScript createWaveScript(WaveRequirements requirements)
        {
            var (blueprint, enemiesPerSpawn, spawnLocations, spawnDuration) = generateWaveParameters(requirements);

            return new WaveScript(
                game.Ids.GetNext<WaveScript>(),
                game.Me.Faction,
                game.State.Time + requirements.DowntimeDuration,
                spawnDuration,
                requirements.Resources,
                spawnLocations,
                enemiesPerSpawn,
                blueprint);
        }

        private (IUnitBlueprint blueprint, int enemiesPerSpawn, ImmutableArray<SpawnLocation> spawnLocations, TimeSpan spawnDuration)
            generateWaveParameters(WaveRequirements requirements)
        {
            var allowedValueError = requirements.WaveValue * WaveValueErrorFactor;
            var minWaveValue = requirements.WaveValue - allowedValueError;
            var maxWaveValue = requirements.WaveValue + allowedValueError;

            var tries = 0;
            IUnitBlueprint blueprint;
            do
            {
                blueprint = selectBlueprint();
            } while (blueprint.Value > maxWaveValue && tries++ < 5);

            var minEnemies = Mathf.CeilToInt(minWaveValue / blueprint.Value);
            var maxEnemies = Mathf.FloorToInt(maxWaveValue / blueprint.Value);
            var numEnemies = maxEnemies <= minEnemies ? minEnemies : random.Next(minEnemies, maxEnemies + 1);

            var activeSpawnLocations = game.State.Enumerate<SpawnLocation>().Where(s => s.IsAwake).ToList();
            State.Satisfies(activeSpawnLocations.Count > 0);

            var minSequentialSpawnTime = numEnemies * MinTimeBetweenSpawns;
            var minSpawnPoints = Mathf.CeilToInt(MaxSpawnTimeDuration / minSequentialSpawnTime);
            var numSpawnPoints = activeSpawnLocations.Count <= minSpawnPoints
                ? minSpawnPoints
                : random.Next(minSpawnPoints, activeSpawnLocations.Count);
            if (numSpawnPoints >= numEnemies)
            {
                numSpawnPoints = numEnemies;
            }
            var spawnLocations = activeSpawnLocations.RandomSubset(numSpawnPoints, random).ToImmutableArray();

            var enemiesPerSpawn = Mathf.CeilToInt((double) numEnemies / numSpawnPoints);
            var actualValue = enemiesPerSpawn * numSpawnPoints * blueprint.Value;
            var actualError = Math.Abs(actualValue - requirements.WaveValue);

            // Try spawning one less enemy per spawn. If that ends up being closer to our desired value, do that.
            var candidateValue = (enemiesPerSpawn - 1) * numSpawnPoints * blueprint.Value;
            var candidateError = Math.Abs(candidateValue - requirements.WaveValue);
            if (candidateValue > 0 && candidateError < actualError)
            {
                enemiesPerSpawn--;
            }

            var spawnDuration = TimeSpan.Min(PreferredTimeBetweenSpawns * enemiesPerSpawn, MaxSpawnTimeDuration);

            return (blueprint, enemiesPerSpawn, spawnLocations, spawnDuration);
        }

        private IUnitBlueprint selectBlueprint()
        {
            var enemies = EnemySpawnDefinition.All;
            var probabilities = new double[enemies.Count + 1];
            foreach (var (enemy, i) in enemies.Indexed())
            {
                probabilities[i + 1] = enemy.Probability + probabilities[i];
            }
            var t = random.NextDouble(probabilities[^1]);
            var result = Array.BinarySearch(probabilities, t);

            var definition = result >= 0 ? enemies[result] : enemies[~result - 1];
            return game.Blueprints.Units[definition.BlueprintId];
        }

        public sealed class WaveRequirements
        {
            public double WaveValue { get; }
            public ResourceAmount Resources { get; }
            public TimeSpan DowntimeDuration { get; }

            public WaveRequirements(double waveValue, ResourceAmount resources, TimeSpan downtimeDuration)
            {
                DowntimeDuration = downtimeDuration;
                Resources = resources;
                WaveValue = waveValue;
            }
        }
    }
}
