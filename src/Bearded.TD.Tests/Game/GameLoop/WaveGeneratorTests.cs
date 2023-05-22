using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.GameLoop;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Testing;
using Bearded.TD.Testing.Enemies;
using Bearded.TD.Testing.Factions;
using Bearded.TD.Tiles;
using Bearded.Utilities.IO;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Game.GameLoop;

public sealed class WaveGeneratorTests
{
    private readonly Logger logger;
    private readonly Faction faction;
    private readonly ImmutableArray<ISpawnableEnemy> enemies;
    private readonly ImmutableArray<IModule> modules;
    private readonly ImmutableArray<SpawnLocation> spawnLocations;

    public WaveGeneratorTests()
    {
        logger = new Logger();
        faction = FactionTestFactory.CreateFaction();
        enemies = ImmutableArray.Create(
            EnemyTestFactory.CreateSpawnableEnemyWithUniqueSocket(out var socketShape1),
            EnemyTestFactory.CreateSpawnableEnemyWithUniqueSocket(out var socketShape2));
        modules = ModuleTestFactory.CreateModulesForAllElements(socketShape1)
            .Concat(ModuleTestFactory.CreateModulesForAllElements(socketShape2)).ToImmutableArray();
        spawnLocations = Enumerable.Range(0, 5)
            .Select(_ => new SpawnLocation(UniqueIds.NextUniqueId<SpawnLocation>(), Tile.Origin))
            .ToImmutableArray();
    }

    [Property]
    public void WaveGenerationIsDeterministicGivenSeed(
        int seed, NonNegativeInt primaryElement, NonNegativeInt accentElement)
    {
        var gen = new WaveGenerator(enemies, modules, seed, logger);
        var elementalTheme = new ElementalTheme(toElement(primaryElement), toElement(accentElement));
        var requirements = new WaveRequirements(1, 1, new WaveEnemyComposition(200, elementalTheme), null);

        var script1 = gen.GenerateWave(requirements, spawnLocations, faction);
        var script2 = gen.GenerateWave(requirements, spawnLocations, faction);

        script1.Should().BeEquivalentTo(script2);
    }

    private static Element toElement(NonNegativeInt i) => (Element) (i.Get % Enum.GetValues<Element>().Length);
}
