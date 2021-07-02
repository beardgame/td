using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using static Bearded.FluentSourceGen.Tests.StaticConfig;

namespace Bearded.FluentSourceGen.Tests
{
    [UsesVerify]
    public sealed class PlaceholderClassTest
    {
        [Fact]
        public Task PlaceholderTest()
        {
            var placeholder = new PlaceholderClass();
            var result = placeholder.PlaceholderMethod();

            return Verifier.Verify(result, DefaultVerifySettings);
        }
    }
}
