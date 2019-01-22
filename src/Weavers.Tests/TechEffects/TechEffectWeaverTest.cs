using FluentAssertions;
using Xunit;

namespace Weavers.Tests.TechEffects
{
    public sealed class TechEffectWeaverTest
    {
        [Fact]
        public void WeaverRunsSuccessfully()
        {
            TechEffectWeaverAssembly.TestResult.Errors.Should().BeEmpty();
        }
    }
}
