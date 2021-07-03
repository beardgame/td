using System.Collections.Immutable;
using System.Threading.Tasks;
using Bearded.TD.Generators.TechEffects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;
using Xunit;
using static Bearded.TD.Generators.Tests.SourceCreator;
using static Bearded.TD.Generators.Tests.StaticConfig;

namespace Bearded.TD.Generators.Tests.TechEffects
{
    [UsesVerify]
    public sealed class TechEffectGeneratorTest
    {
        [Fact]
        public async Task GeneratesProxyForInterface()
        {
            var syntaxTree = await SyntaxTreeFromRelativeFile("testdata/IMyParameters.cs");
            var compilation = CSharpCompilation.Create("compilation", ImmutableArray.Create(syntaxTree), References);
            var generator = new TechEffectGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGenerators(compilation);

            await Verifier.Verify(driver, DefaultVerifySettings);
        }
    }
}
