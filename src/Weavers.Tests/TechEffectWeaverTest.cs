using System;
using System.Collections.Generic;
using System.Reflection;
using Bearded.TD.Shared.TechEffects;
using FluentAssertions;
using Fody;
using Newtonsoft.Json;
using Weavers.TechEffects;
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
        public void MakesTemplateTypeRememberSimpleValues()
        {
            var template = constructTemplate(42, null, null);

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntProperty)).Should().Be(42);
        }

        [Fact]
        public void MakesTemplateTypeRememberValuesWithDefaultValues()
        {
            var template = constructTemplate(0, (int?) 37, null);

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(37);
        }

        [Fact]
        public void MakesTemplateTypeRememberWrappedValues()
        {
            // ReSharper disable once PossibleNullReferenceException
            var template = constructTemplate(0, null, getWrappedIntType().GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 18 }));

            template
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(18);
        }

        [Fact]
        public void JsonDeserializesSimpleTypes()
        {
            const string json = "{ \"intProperty\" : 42 }";

            var template = JsonConvert.DeserializeObject(json, getTemplateType());

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntProperty)).Should().Be(42);
        }

        [Fact]
        public void JsonDeserializesDefaultsForSimpleTypes()
        {
            const string json = "{ \"intProperty\" : 0 }";

            var template = JsonConvert.DeserializeObject(json, getTemplateType());

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }

        [Fact]
        public void JsonDeserializesSimpleTypesWithDefault()
        {
            const string json = "{ \"intProperty\" : 0, \"intPropertyWithDefault\" : 37 }";

            var template = JsonConvert.DeserializeObject(json, getTemplateType());

            template.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(37);
        }

        [Fact]
        public void RegistersTemplateType()
        {
            var libraryInstance = getTechEffectLibraryType()?.InvokeMember(
                nameof(ParametersTemplateLibrary.Instance),
                BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                null, null, null);
            // ReSharper disable once PossibleNullReferenceException
            var dict = getTechEffectLibraryType()
                .GetMethod(nameof(ParametersTemplateLibrary.GetInterfaceToTemplateMap))
                .Invoke(libraryInstance, null) as IDictionary<Type, Type>;

            dict.Should().Contain(getInterfaceType(), getTemplateType());
        }

        [Fact]
        public void InjectsModifiableType()
        {
            getModifiableType().Should().NotBeNull();
        }

        [Fact]
        public void MakesModifiableTypeImplementInterface()
        {
            getModifiableType().Should().BeAssignableTo(getInterfaceType());
        }

        [Fact]
        public void MakesModifiableTypeConstructable()
        {
            getModifiableConstructorInfo().Should().NotBeNull();
        }

        [Fact]
        public void MakesModifiableTypeRememberSimpleValues()
        {
            var modifiable = constructorModifiable(constructTemplate(42, null, null));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntProperty)).Should().Be(42);
        }

        [Fact]
        public void MakesModifiableTypeRememberModifiableValues()
        {
            var modifiable = constructorModifiable(constructTemplate(0, 10, null));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }
        
        [Fact]
        public void MakesModifiableTypeRememberModifiableWrappedValues()
        {
            var modifiable = constructorModifiable(
                constructTemplate(
                    0,
                    null,
                    getWrappedIntType().GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 18 })));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.WrappedIntProperty.Val)).Should().Be(18);
        }
        
        [Fact]
        public void MakesModifiableTypeAbleToModifyValues()
        {
            var modifiable = constructorModifiable(constructTemplate(0, 10, null));

            modifiable.CallMethod(nameof(ModifiableBase.ModifyAttribute), AttributeType.DamagePerUnit,
                new Modification(Modification.ModificationType.Multiplicative, 1));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(20);
        }
        
        [Fact]
        public void MakesModifiableTypeAbleToModifyWrappedValues()
        {
            var modifiable = constructorModifiable(
                constructTemplate(
                    0,
                    null,
                    getWrappedIntType().GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 18 })));
            
            modifiable.CallMethod(nameof(ModifiableBase.ModifyAttribute), AttributeType.Cooldown,
                new Modification(Modification.ModificationType.Multiplicative, 1));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.WrappedIntProperty.Val)).Should().Be(36);
        }

        private static object constructTemplate(params object[] constructorParams)
        {
            return getTemplateConstructorInfo().Invoke(constructorParams);
        }

        private static object constructorModifiable(object template)
        {
            return getModifiableConstructorInfo().Invoke(new[] { template });
        }

        private static ConstructorInfo getTemplateConstructorInfo()
        {
            var cs = getTemplateType().GetConstructors();
            return cs.Length == 1 ? cs[0] : null;
        }

        private static ConstructorInfo getModifiableConstructorInfo()
        {
            var cs = getModifiableType().GetConstructors();
            return cs.Length == 1 ? cs[0] : null;
        }

        private static Type getInterfaceType() => getAssemblyType(nameof(IDummyParametersTemplate));
        private static Type getTemplateType() => getAssemblyType("DummyParametersTemplate");
        private static Type getModifiableType() => getAssemblyType("DummyParametersModifiable");
        private static Type getTechEffectLibraryType() => getAssemblyType(nameof(ParametersTemplateLibrary));
        private static Type getWrappedIntType() => getAssemblyType(nameof(WrappedInt));

        private static Type getAssemblyType(string name)
        {
            return testResult.Assembly.GetType($"{Constants.NameSpace}.{name}");
        }
    }
}
