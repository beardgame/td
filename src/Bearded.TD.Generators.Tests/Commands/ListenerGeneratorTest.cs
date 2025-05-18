using System.Collections.Immutable;
using System.Threading.Tasks;
using Bearded.TD.Generators.Serializers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;
using Xunit;
using static Bearded.TD.Generators.Tests.SourceCreator;
using static Bearded.TD.Generators.Tests.StaticConfig;

namespace Bearded.TD.Generators.Tests.Commands
{
    public sealed class CommandGeneratorTest
    {
        [Fact]
        public async Task GeneratesListenerPartialClass()
        {
            var syntaxTrees = ImmutableArray.Create(
                await SyntaxTreeFromRelativeFile("testdata/EmptyCommand.cs"),
                await SyntaxTreeFromRelativeFile("testdata/SimpleCommand.cs"),
                await SyntaxTreeFromRelativeFile("testdata/ComplexCommand.cs")
            );
            var compilation = CSharpCompilation.Create("compilation", syntaxTrees, References);
            var generator = new CommandGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGenerators(compilation);

            await Verifier.Verify(driver, DefaultVerifySettings);
        }
    }
}
