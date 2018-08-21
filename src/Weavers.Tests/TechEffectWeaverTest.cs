using System;
using System.Reflection;
using Fody;
using Weavers.Tests.AssemblyToProcess;
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
            Assert.NotNull(getTemplateType());
        }

        [Fact]
        public void MakesTemplateTypeConstructable()
        {
            Assert.NotNull(getTemplateConstructorInfo());
        }

        [Fact]
        public void MakesTemplateTypeRememberValues()
        {
            var template = constructTemplate(42);
            Assert.Equal(42, template.IntProperty);
        }

        private static ITechEffectDummy constructTemplate(int intValue)
        {
            return getTemplateConstructorInfo().Invoke(new object[] {intValue}) as ITechEffectDummy;
        }

        private static ConstructorInfo getTemplateConstructorInfo()
        {
            return getTemplateType().GetConstructor(new[] {typeof(int)});
        }

        private static Type getTemplateType()
        {
            return testResult.Assembly.GetType($"{Constants.NameSpace}.TechEffectModifiableTemplate");
        }
    }
}
