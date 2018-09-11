using System;
using System.Reflection;
using FluentAssertions;
using Fody;
using Newtonsoft.Json;
using Weavers.TechEffects;
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
            testResult.Errors.Should().BeEmpty();
        }

        [Fact]
        public void InjectsTemplateType()
        {
            getTemplateType().Should().NotBeNull();
        }

        [Fact]
        public void MakesTemplateTypeImplementInterface()
        {
            getTemplateType().Should().BeAssignableTo(getInterfaceType());
        }

        [Fact]
        public void MakesTemplateTypeConstructable()
        {
            getTemplateConstructorInfo().Should().NotBeNull();
        }

        [Fact]
        public void MakesTemplateTypeRememberValues()
        {
            var template = constructTemplate(42);

            template.GetPropertyValue<int>("IntProperty").Should().Be(42);
        }

        [Fact]
        public void MakesTemplateTypeJsonDeserializable()
        {
            const string json = "{ \"intProperty\" : 42 }";

            var template = JsonConvert.DeserializeObject(json, getTemplateType());

            template.GetPropertyValue<int>("IntProperty").Should().Be(42);
        }

        private static object constructTemplate(int intValue)
        {
            return getTemplateConstructorInfo().Invoke(new object[] {intValue});
        }

        private static ConstructorInfo getTemplateConstructorInfo()
        {
            return getTemplateType().GetConstructor(new[] {typeof(int)});
        }

        private static Type getInterfaceType()
        {
            return testResult.Assembly.GetType($"{Constants.NameSpace}.ITechEffectDummy");
        }

        private static Type getTemplateType()
        {
            return testResult.Assembly.GetType($"{Constants.NameSpace}.TechEffectDummyTemplate");
        }
    }
}
