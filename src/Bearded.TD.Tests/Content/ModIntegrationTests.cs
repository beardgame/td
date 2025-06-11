using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bearded.TD.Content.Mods;
using Bearded.TD.Content.Serialization.Models;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Bearded.TD.Tests.Content;

public sealed class ModIntegrationTests : IAsyncLifetime
{
    private const string modForTestingPath = "assets/mods/mod-for-testing";

    private Mod testMod = null!;

    public async Task InitializeAsync()
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

        var testModMetadataJson =
            JsonConvert.DeserializeObject<Metadata>(await File.ReadAllTextAsync($"{modForTestingPath}/mod.json5"))!;
        var testModMetadata = new ModMetadata(testModMetadataJson, new DirectoryInfo(modForTestingPath));
        var testModForLoading = new ModForLoading(testModMetadata);
        await testModForLoading.Load(context, loadedMods.AsReadOnly());
        testMod = testModForLoading.GetLoadedMod();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public void ModIsLoaded()
    {
        testMod.Name.Should().Be("Mod for testing");
    }
}
