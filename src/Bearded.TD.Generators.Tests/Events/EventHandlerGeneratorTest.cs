using System.Collections.Immutable;
using System.Threading.Tasks;
using Bearded.TD.Generators.Events;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;
using Xunit;
using static Bearded.TD.Generators.Tests.SourceCreator;
using static Bearded.TD.Generators.Tests.StaticConfig;

namespace Bearded.TD.Generators.Tests.Events
{
    public sealed class EventHandlerGeneratorTest
    {
        [Fact]
        public async Task GeneratesEventsPartialClass()
        {
            var syntaxTrees = ImmutableArray.Create(
                await SyntaxTreeFromRelativeFile("testdata/EventImplementations.cs"),
                await SyntaxTreeFromRelativeFile("testdata/ListeningComponent.cs")
            );
            var compilation = CSharpCompilation.Create("compilation", syntaxTrees, References);
            var generator = new EventHandlerGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGenerators(compilation);

            await Verifier.Verify(driver, DefaultVerifySettings);
        }
    }
}
