using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Bearded.TD.Content.Mods;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Content;

public sealed class ModLoadingIntegrationTest
{
    [Fact]
    public async Task AllModsLoadSuccessfully()
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

        context.Errors.Should().BeEmpty("no errors should be thrown by any blueprints");
    }
}
