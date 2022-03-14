using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop;

sealed class WaveScheduler : IListener<WaveEnded>
{
    private readonly Random random = new();
    private readonly GameState game;
    private readonly Faction targetFaction;
    private readonly ICommandDispatcher<GameInstance> commandDispatcher;
    public event VoidEventHandler? WaveEnded;

    private Id<WaveScript>? activeWave;

    public WaveScheduler(GameState game, Faction targetFaction, ICommandDispatcher<GameInstance> commandDispatcher)
    {
        this.game = game;
        this.targetFaction = targetFaction;
        this.commandDispatcher = commandDispatcher;
    }

    public void OnGameStart()
    {
        game.Meta.Events.Subscribe(this);
    }

    public void HandleEvent(WaveEnded @event)
    {
        if (@event.WaveId != activeWave)
        {
            return;
        }

        activeWave = null;
        WaveEnded?.Invoke();
    }

    public void StartWave(WaveRequirements requirements)
    {
        State.Satisfies(activeWave == null, "We only support one simultaneous wave right now");

        var dormantSpawnLocations = game.Enumerate<SpawnLocation>().Where(s => !s.IsAwake).ToList();
        if (dormantSpawnLocations.Count > 0)
        {
            commandDispatcher.Dispatch(WakeUpSpawnLocation.Command(dormantSpawnLocations.RandomElement()));
        }

        var script = createWaveScript(requirements);
        activeWave = script.Id;
        commandDispatcher.Dispatch(ExecuteWaveScript.Command(game, script));
    }

    private WaveScript createWaveScript(WaveRequirements requirements)
    {
        var (blueprint, enemiesPerSpawn, spawnLocations, spawnDuration) = generateWaveParameters(requirements);

        return new WaveScript(
            game.Meta.Ids.GetNext<WaveScript>(),
            $"Ch {requirements.ChapterNumber}; Wave {requirements.WaveNumber}",
            targetFaction,
            game.Time + requirements.DowntimeDuration,
            spawnDuration,
            requirements.Resources,
            spawnLocations,
            enemiesPerSpawn,
            blueprint,
            game.Meta.Ids.GetBatch<ComponentGameObject>(spawnLocations.Length * enemiesPerSpawn));
    }

    private WaveParameters generateWaveParameters(WaveRequirements requirements)
    {
        var allowedValueError = requirements.WaveValue * WaveValueErrorFactor;
        var minWaveValue = requirements.WaveValue - allowedValueError;
        var maxWaveValue = requirements.WaveValue + allowedValueError;

        var tries = 0;
        IComponentOwnerBlueprint blueprint;
        float blueprintThreat;
        do
        {
            blueprint = selectBlueprint();
            blueprintThreat = blueprint.GetThreat();
        } while (blueprintThreat > maxWaveValue && tries++ < 10);

        var minEnemies = MoreMath.CeilToInt(minWaveValue / blueprintThreat);
        var maxEnemies = MoreMath.FloorToInt(maxWaveValue / blueprintThreat);
        var numEnemies = maxEnemies <= minEnemies ? minEnemies : random.Next(minEnemies, maxEnemies + 1);

        var activeSpawnLocations = game.Enumerate<SpawnLocation>().Where(s => s.IsAwake).ToList();
        State.Satisfies(activeSpawnLocations.Count > 0);

        var minSequentialSpawnTime = numEnemies * MinTimeBetweenSpawns;
        var minSpawnPoints = MoreMath.CeilToInt(EnemyTrainLength / (minSequentialSpawnTime * numEnemies));
        var numSpawnPoints = activeSpawnLocations.Count <= minSpawnPoints
            ? minSpawnPoints
            : random.Next(minSpawnPoints, activeSpawnLocations.Count);
        if (numSpawnPoints >= numEnemies)
        {
            numSpawnPoints = numEnemies;
        }
        var spawnLocations = activeSpawnLocations.RandomSubset(numSpawnPoints, random).ToImmutableArray();

        var enemiesPerSpawn = MoreMath.CeilToInt((double) numEnemies / numSpawnPoints);
        var actualValue = enemiesPerSpawn * numSpawnPoints * blueprintThreat;
        var actualError = Math.Abs(actualValue - requirements.WaveValue);

        // Try spawning one less enemy per spawn. If that ends up being closer to our desired value, do that.
        var candidateValue = (enemiesPerSpawn - 1) * numSpawnPoints * blueprintThreat;
        var candidateError = Math.Abs(candidateValue - requirements.WaveValue);
        if (candidateValue > 0 && candidateError < actualError)
        {
            enemiesPerSpawn--;
        }

        var spawnDuration = TimeSpan.Max(
            TimeSpan.Min(
                EnemyTrainLength,
                MaxTimeBetweenSpawns * (enemiesPerSpawn - 1)),
            MinTimeBetweenSpawns * (enemiesPerSpawn - 1));

        return new WaveParameters(blueprint, enemiesPerSpawn, spawnLocations, spawnDuration);
    }

    private record struct WaveParameters(
        IComponentOwnerBlueprint UnitBlueprint,
        int EnemiesPerSpawn,
        ImmutableArray<SpawnLocation> SpawnLocations,
        TimeSpan SpawnDuration);

    private IComponentOwnerBlueprint selectBlueprint()
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
        return game.Meta.Blueprints.ComponentOwners[definition.BlueprintId];
    }

    public sealed class WaveRequirements
    {
        public int ChapterNumber { get; }
        public int WaveNumber { get; }
        public double WaveValue { get; }
        public ResourceAmount Resources { get; }
        public TimeSpan DowntimeDuration { get; }

        public WaveRequirements(
            int chapterNumber,
            int waveNumber,
            double waveValue,
            ResourceAmount resources,
            TimeSpan downtimeDuration)
        {
            ChapterNumber = chapterNumber;
            WaveNumber = waveNumber;
            DowntimeDuration = downtimeDuration;
            Resources = resources;
            WaveValue = waveValue;
        }
    }
}
