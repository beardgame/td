using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using static Bearded.TD.Constants.Game.WaveGeneration;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveScheduler : IListener<WaveEnded>
{
    private readonly Random random = new();
    private readonly GameState game;
    private readonly Faction targetFaction;
    private readonly ICommandDispatcher<GameInstance> commandDispatcher;
    private readonly Logger logger;
    public event VoidEventHandler? WaveEnded;

    private Id<WaveScript>? activeWave;

    public WaveScheduler(
        GameState game, Faction targetFaction, ICommandDispatcher<GameInstance> commandDispatcher, Logger logger)
    {
        this.game = game;
        this.targetFaction = targetFaction;
        this.commandDispatcher = commandDispatcher;
        this.logger = logger;
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
            game.Meta.Ids.GetBatch<GameObject>(spawnLocations.Length * enemiesPerSpawn));
    }

    private WaveParameters generateWaveParameters(WaveRequirements requirements)
    {
        logger.Debug?.Log($"Wave parameters requested with threat {requirements.WaveValue}");
        var sw = Stopwatch.StartNew();

        var (minWaveValue, maxWaveValue) = waveValueRange(requirements);

        var (blueprint, blueprintThreat, enemyCount) = chooseEnemy(maxWaveValue, minWaveValue);

        var spawnLocations = chooseSpawnLocations(enemyCount);
        var spawnLocationCount = spawnLocations.Length;

        var enemiesPerSpawn = MoreMath.CeilToInt((double) enemyCount / spawnLocationCount);

        optimizeForWaveValue(requirements, spawnLocationCount, blueprintThreat, ref enemiesPerSpawn);

        var spawnDuration = TimeSpan.Max(
            TimeSpan.Min(
                EnemyTrainLength,
                MaxTimeBetweenSpawns * (enemiesPerSpawn - 1)),
            MinTimeBetweenSpawns * (enemiesPerSpawn - 1));

        logger.Debug?.Log(
            $"Generated wave parameters with {enemiesPerSpawn} enemies of threat {blueprintThreat} at " +
            $"{spawnLocations.Length} spawn locations for a total of " +
            $"{enemiesPerSpawn * spawnLocations.Length * blueprintThreat} threat in {sw.Elapsed.TotalSeconds}s");

        return new WaveParameters(blueprint, enemiesPerSpawn, spawnLocations, spawnDuration);
    }

    private static (double minWaveValue, double maxWaveValue) waveValueRange(WaveRequirements requirements)
    {
        var allowedValueError = requirements.WaveValue * WaveValueErrorFactor;
        var minWaveValue = requirements.WaveValue - allowedValueError;
        var maxWaveValue = requirements.WaveValue + allowedValueError;
        return (minWaveValue, maxWaveValue);
    }

    private static void optimizeForWaveValue(
        WaveRequirements requirements, int spawnLocationCount, float blueprintThreat, ref int enemiesPerSpawn)
    {
        var actualValue = enemiesPerSpawn * spawnLocationCount * blueprintThreat;
        var actualError = Math.Abs(actualValue - requirements.WaveValue);

        // Try spawning one less enemy per spawn. If that ends up being closer to our desired value, do that.
        var candidateValue = (enemiesPerSpawn - 1) * spawnLocationCount * blueprintThreat;
        var candidateError = Math.Abs(candidateValue - requirements.WaveValue);
        if (candidateValue > 0 && candidateError < actualError)
        {
            enemiesPerSpawn--;
        }
    }

    private record struct WaveParameters(
        IGameObjectBlueprint UnitBlueprint,
        int EnemiesPerSpawn,
        ImmutableArray<SpawnLocation> SpawnLocations,
        TimeSpan SpawnDuration);

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
