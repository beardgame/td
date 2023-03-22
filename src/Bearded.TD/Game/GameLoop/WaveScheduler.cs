using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.GameObjects;
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
    private readonly GameState game;
    private readonly Faction targetFaction;
    private readonly ImmutableArray<ISpawnableEnemy> spawnableEnemies;
    private readonly ICommandDispatcher<GameInstance> commandDispatcher;
    private readonly Random random;
    private readonly Logger logger;
    private readonly EnemyFormGenerator enemyFormGenerator;
    public event VoidEventHandler? WaveEnded;

    private Id<WaveScript>? activeWave;

    public WaveScheduler(
        GameState game,
        Faction targetFaction,
        ImmutableArray<ISpawnableEnemy> spawnableEnemies,
        ICommandDispatcher<GameInstance> commandDispatcher,
        int seed,
        Logger logger)
    {
        this.game = game;
        this.targetFaction = targetFaction;
        this.spawnableEnemies = spawnableEnemies;
        this.commandDispatcher = commandDispatcher;
        random = new Random(seed);
        this.logger = logger;
        enemyFormGenerator = new EnemyFormGenerator(game.Meta.Blueprints.Modules.All, random, logger);
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
        var (enemyForm, enemiesPerSpawn, spawnLocations, spawnDuration) = generateWaveParameters(requirements);

        var enemyScript = toEnemyScript(enemiesPerSpawn, spawnDuration, enemyForm);

        return new WaveScript(
            game.Meta.Ids.GetNext<WaveScript>(),
            $"Ch {requirements.ChapterNumber}; Wave {requirements.WaveNumber}",
            targetFaction,
            requirements.DowntimeDuration == null ? null : game.Time + requirements.DowntimeDuration,
            spawnDuration,
            requirements.Resources,
            spawnLocations,
            enemyScript,
            game.Meta.Ids.GetBatch<GameObject>(spawnLocations.Length * enemiesPerSpawn));
    }

    private WaveParameters generateWaveParameters(WaveRequirements requirements)
    {
        logger.Debug?.Log($"Wave parameters requested with threat {requirements.EnemyComposition.TotalThreat}");
        var sw = Stopwatch.StartNew();

        var (minWaveValue, maxWaveValue) = totalThreatRange(requirements);
        var element = requirements.EnemyComposition.Elements.PrimaryElement;

        var (blueprint, blueprintThreat, enemyCount) = chooseEnemy(element, minWaveValue, maxWaveValue);

        var spawnLocations = chooseSpawnLocations(enemyCount);
        var spawnLocationCount = spawnLocations.Length;

        var enemiesPerSpawn = MoreMath.CeilToInt((double) enemyCount / spawnLocationCount);

        optimizeForRequestedThreat(requirements, spawnLocationCount, blueprintThreat, ref enemiesPerSpawn);

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

    private static (double minThreat, double maxThreat) totalThreatRange(WaveRequirements requirements)
    {
        var requestedThreat = requirements.EnemyComposition.TotalThreat;
        var allowedValueError = requestedThreat * WaveValueErrorFactor;
        var minWaveValue = requestedThreat - allowedValueError;
        var maxWaveValue = requestedThreat + allowedValueError;
        return (minWaveValue, maxWaveValue);
    }

    private static void optimizeForRequestedThreat(
        WaveRequirements requirements, int spawnLocationCount, float blueprintThreat, ref int enemiesPerSpawn)
    {
        var requestedThreat = requirements.EnemyComposition.TotalThreat;

        var actualValue = enemiesPerSpawn * spawnLocationCount * blueprintThreat;
        var actualError = Math.Abs(actualValue - requestedThreat);

        // Try spawning one less enemy per spawn. If that ends up being closer to our desired value, do that.
        var candidateValue = (enemiesPerSpawn - 1) * spawnLocationCount * blueprintThreat;
        var candidateError = Math.Abs(candidateValue - requestedThreat);
        if (candidateValue > 0 && candidateError < actualError)
        {
            enemiesPerSpawn--;
        }
    }

}
