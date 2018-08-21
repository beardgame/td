using Fody;
using Xunit;
#pragma warning disable 618 // Disable obsolete warnings

namespace Weavers.Tests
{
    public sealed class TDWeaverTest
    {
        private static readonly TestResult testResult;

        static TDWeaverTest()
        {
            var weavingTask = new TDWeaver();
            testResult = weavingTask.ExecuteTestRun(Constants.AssemblyToProcess);
        }

        [Fact]
        public void RunsSuccessfully()
        {
            Assert.Empty(testResult.Errors);
        }

        [Fact]
        public void InjectsType()
        {
            // Should not fail
            testResult.Assembly.GetType("Weavers.TypeInjectedByTDWeaver");
        }
    }
}
