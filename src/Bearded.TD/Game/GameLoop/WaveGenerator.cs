using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.Utilities.IO;
using static Bearded.TD.Game.GameLoop.WaveStructure;

namespace Bearded.TD.Game.GameLoop;

sealed partial class WaveGenerator
{
    private readonly ImmutableArray<ISpawnableEnemy> spawnableEnemies;
    private readonly Faction targetFaction;
    private readonly int seed;
    private readonly EnemyFormGenerator enemyFormGenerator;

    public WaveGenerator(
        ImmutableArray<ISpawnableEnemy> spawnableEnemies,
        IEnumerable<IModule> modules,
        Faction targetFaction,
        int seed,
        Logger logger)
    {
        this.spawnableEnemies = spawnableEnemies;
        this.targetFaction = targetFaction;
        this.seed = seed;
        enemyFormGenerator = new EnemyFormGenerator(modules, logger);
    }

    public WaveScript GenerateWave(
        WaveRequirements requirements,
        IEnumerable<SpawnLocation> availableSpawnLocations)
    {
        // Ensure that a change in requirements always leads to a (very) different outcome.
        var random = new Random(seed ^ requirements.GetHashCode());

        var enemyScript = generateScript(requirements.Structure, availableSpawnLocations, random);
        var spawnLocations = enemyScript.SpawnEvents.Select(e => e.SpawnLocation).Distinct().ToImmutableArray();

        return new WaveScript(
            requirements.ChapterNumber,
            requirements.WaveNumber,
            requirements.IsFinalWave,
            $"Ch {requirements.ChapterNumber}; Wave {requirements.WaveNumber}",
            targetFaction,
            requirements.DowntimeDuration,
            spawnLocations,
            enemyScript);
    }

    private EnemySpawnScript generateScript(
        ScriptStructure structure,
        IEnumerable<SpawnLocation> availableSpawnLocations,
        Random random)
    {
        var routineCompositions =
            structure.Routines.Select(routine => generateRoutineComposition(routine, random)).ToImmutableArray();
        var assignedSpawnLocations = assignSpawnLocations(availableSpawnLocations, routineCompositions, random);
        var routineScripts =
            routineCompositions.Select(comp => generateSpawnScript(comp, assignedSpawnLocations[comp], random));

        // TODO: allow offsetting the routines
        return EnemySpawnScript.Merge(routineScripts);
    }
}
