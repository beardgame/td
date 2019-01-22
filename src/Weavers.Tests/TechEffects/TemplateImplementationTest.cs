using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using Weavers.Tests.AssemblyToProcess;
using Xunit;

namespace Weavers.Tests.TechEffects
{
    public sealed class TemplateImplementationTest : ImplementationTestBase
    {
        protected override Type ImplementationType => TemplateType;
        
        [Fact]
        public void RemembersSimpleValues()
        {
            var template = ConstructTemplate(42, null, null);

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntProperty)).Should().Be(42);
        }

        [Fact]
        public void RemembersValuesWithDefaultValues()
        {
            var template = ConstructTemplate(0, (int?) 37, null);

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(37);
        }

        [Fact]
        public void RemembersWrappedValues()
        {
            // ReSharper disable once PossibleNullReferenceException
            var template = ConstructTemplate(
                0, null, WrappedIntType.GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 18 }));

            template
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(18);
        }

        [Fact]
        public void JsonDeserializesSimpleTypes()
        {
            const string json = "{ \"intProperty\" : 42 }";

            var template = JsonConvert.DeserializeObject(json, TemplateType);

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntProperty)).Should().Be(42);
        }

        [Fact]
        public void JsonDeserializesDefaultsForSimpleTypes()
        {
            const string json = "{ \"intProperty\" : 0 }";

            var template = JsonConvert.DeserializeObject(json, TemplateType);

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }

        [Fact]
        public void JsonDeserializesSimpleTypesWithDefault()
        {
            const string json = "{ \"intProperty\" : 0, \"intPropertyWithDefault\" : 37 }";

            var template = JsonConvert.DeserializeObject(json, TemplateType);

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(37);
        }

        [Fact]
        public void IsRegisteredInLibrary()
        {
            var libraryInstance = TechEffectLibraryType?.InvokeMember(
                nameof(ParametersTemplateLibrary.Instance),
                BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                null, null, null);
            // ReSharper disable once PossibleNullReferenceException
            var dict = TechEffectLibraryType
                .GetMethod(nameof(ParametersTemplateLibrary.GetInterfaceToTemplateMap))
                .Invoke(libraryInstance, null) as IDictionary<Type, Type>;

            dict.Should().Contain(InterfaceType, TemplateType);
        }
    }
}
