using Fody;
using Xunit;
#pragma warning disable 618 // Disable obsolete warnings

namespace Weavers.Tests
{
    public sealed class TechEffectWeaverTest
    {
        private static readonly TestResult testResult;

        static TechEffectWeaverTest()
        {
            var weavingTask = new TechEffectWeaver();
            testResult = weavingTask.ExecuteTestRun(Constants.AssemblyToProcess);
        }

        [Fact]
        public void RunsSuccessfully()
        {
            Assert.Empty(testResult.Errors);
        }

        [Fact]
        public void InjectsTemplateType()
        {
            Assert.NotNull(testResult.Assembly.GetType($"{Constants.NameSpace}.TechEffectModifiableTemplate"));
        }
    }
}
