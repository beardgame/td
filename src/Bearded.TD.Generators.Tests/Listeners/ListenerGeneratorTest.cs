using System.Collections.Immutable;
using System.Threading.Tasks;
using Bearded.TD.Generators.Listeners;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;
using Xunit;
using static Bearded.TD.Generators.Tests.SourceCreator;
using static Bearded.TD.Generators.Tests.StaticConfig;

namespace Bearded.TD.Generators.Tests.Listeners
{
    public sealed class ListenerGeneratorTest
    {
        [Fact]
        public async Task GeneratesListenerPartialClass()
        {
            var syntaxTrees = ImmutableArray.Create(
                await SyntaxTreeFromRelativeFile("testdata/EventImplementations.cs"),
                await SyntaxTreeFromRelativeFile("testdata/MyEventListener.cs"),
                await SyntaxTreeFromRelativeFile("testdata/MyEvents.cs")
            );
            var compilation = CSharpCompilation.Create("compilation", syntaxTrees, References);
            var generator = new ListenerGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGenerators(compilation);

            await Verifier.Verify(driver, DefaultVerifySettings);
        }
    }
}
