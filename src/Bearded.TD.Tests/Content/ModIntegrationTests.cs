using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.Utilities.SpaceTime;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Content;

public sealed class ModIntegrationTests : IAsyncLifetime
{
    private const string testModId = "mod-for-testing";

    private Mod testMod = null!;
    private Blueprints blueprints = null!;

    public async ValueTask InitializeAsync()
    {
        var allMods = new ModLister().GetAll().ToImmutableArray();
        var sortedMods = new ModSorter().SortByDependency(allMods);
        var context = ModLoadingMocks.CreateLoadingContext();

        var loadedMods = new List<Mod>();

        foreach (var modForLoading in sortedMods.Select(modMetadata => new ModForLoading(modMetadata)))
        {
            await modForLoading.Load(context, loadedMods.AsReadOnly());
            if (!modForLoading.DidLoadSuccessfully)
            {
                modForLoading.Rethrow();
            }
            loadedMods.Add(modForLoading.GetLoadedMod());
        }

        testMod = loadedMods.Single(m => m.Id == testModId);
        blueprints = Blueprints.Merge(loadedMods.Select(m => m.Blueprints));
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public void ModIsLoaded()
    {
        testMod.Name.Should().Be("Mod for testing");
    }

    [Fact]
    public void EnemyFormRecipeIsLoaded()
    {
        var spawnEnemiesOnKilled = gameObjectInstance("spawn-enemies-on-killed");

        var component = spawnEnemiesOnKilled.GetComponents<SpawnEnemiesOnKilled>().SingleOrDefault();

        component.Should().NotBeNull();
        var parameters = extractParameters(component);
        var form = parameters.Form.ToForm(blueprints);
        form.Blueprint.Should().Be(blueprints.GameObjects[ModAwareId.FromFullySpecified("default.swarm-enemy")]);
        form.Modules[SocketShape.FromLiteral("minionBuff")].Should()
            .Be(blueprints.Modules[ModAwareId.FromFullySpecified("default.minion-buff-fire")]);
    }

    private GameObject gameObjectInstance(string name) =>
        GameObjectFactory.CreateFromBlueprintWithoutRenderer(gameObjectBlueprint(name), null, Position3.Zero);

    private IGameObjectBlueprint gameObjectBlueprint(string name) => blueprints.GameObjects[id(name)];

    private ModAwareId id(string name) => ModAwareId.FromFullySpecified($"{testMod.Id}.{name}");

    private static T extractParameters<T>(Component<T> component) where T : IParametersTemplate<T>
    {
        var parametersProperty =
            component.GetType().GetProperty("Parameters", BindingFlags.Instance | BindingFlags.NonPublic);
        return (T) parametersProperty!.GetValue(component)!;
    }
}
