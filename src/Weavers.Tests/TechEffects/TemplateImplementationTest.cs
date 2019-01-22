using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Shared.TechEffects;
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
            var template = ConstructTemplate(0, null, ConstructWrappedInt(18));

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
            // ReSharper disable PossibleNullReferenceException
            var dict = TechEffectLibraryType
                .GetMethod(nameof(ParametersTemplateLibrary.GetInterfaceToTemplateMap))
                .Invoke(libraryInstance, null) as IDictionary<Type, Type>;
            // ReSharper restore PossibleNullReferenceException

            dict.Should().Contain(InterfaceType, TemplateType);
        }

        [Fact]
        public void HasAttributeOfType_AttributeOfTypePresent_ReturnsTrue()
        {
            var template = ConstructTemplate(42, null, null);

            ((bool) template.CallMethod(HasAttributeOfTypeMethodName, AttributeType.Damage)).Should().BeTrue();
        }

        [Fact]
        public void HasAttributeOfType_AttributeOfTypeAbsent_ReturnsFalse()
        {
            var template = ConstructTemplate(42, null, null);

            ((bool) template.CallMethod(HasAttributeOfTypeMethodName, AttributeType.DroneCount)).Should().BeFalse();
        }

        [Fact]
        public void ModifyAttribute_ThrowsException()
        {
            var template = ConstructTemplate(42, null, null);

            template
                .Invoking(obj => obj.CallMethod(
                    ModifyAttributeMethodName, AttributeType.Damage, Modification.AddFractionOfBase(1)))
                .Should()
                .Throw<Exception>() // indirection because of reflection call
                .WithInnerException<InvalidOperationException>();
        }

        [Fact]
        public void CreateModifiableInstance_ReturnsAModifiableInstance()
        {
            var template = ConstructTemplate(42, null, null);

            template.CallMethod(CreateModifiableInstanceMethodName).Should().BeAssignableTo(ModifiableType);
        }

        [Fact]
        public void CreateModifiableInstance_CopiesValuesFromTemplate()
        {
            var template = ConstructTemplate(42, 10, ConstructWrappedInt(18));

            var modifiable = template.CallMethod(CreateModifiableInstanceMethodName);
            
            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntProperty)).Should().Be(42);
            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(18);
        }
    }
}
