using System;
using Bearded.TD.Shared.TechEffects;
using FluentAssertions;
using Weavers.Tests.AssemblyToProcess;
using Xunit;

namespace Weavers.Tests.TechEffects
{
    public sealed class ModifiableImplementationTest : ImplementationTestBase
    {
        protected override Type ImplementationType => ModifiableType;
        
        [Fact]
        public void RemembersSimpleValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(42, null, null));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntProperty)).Should().Be(42);
        }

        [Fact]
        public void RemembersModifiableValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }
        
        [Fact]
        public void RemembersModifiableWrappedValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, null, ConstructWrappedInt(18)));
            
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(18);
        }

        [Fact]
        public void HasAttributeOfType_AttributeOfTypePresent_ReturnsTrue()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(42, null, null));

            ((bool) modifiable.CallMethod(HasAttributeOfTypeMethodName, AttributeType.Damage)).Should().BeTrue();
        }

        [Fact]
        public void HasAttributeOfType_AttributeOfTypeAbsent_ReturnsFalse()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(42, null, null));

            ((bool) modifiable.CallMethod(HasAttributeOfTypeMethodName, AttributeType.DroneCount)).Should().BeFalse();
        }
        
        [Fact]
        public void ModifyAttribute_ModifiesSimpleValue()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            modifiable.CallMethod(
                ModifyAttributeMethodName, AttributeType.Damage, Modification.AddFractionOfBase(1));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(20);
        }
        
        [Fact]
        public void ModifyAttribute_ModifiesWrappedValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, null, ConstructWrappedInt(18)));
            
            modifiable.CallMethod(
                ModifyAttributeMethodName, AttributeType.Cooldown, Modification.AddFractionOfBase(1));
            
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(36);
        }
        
        [Fact]
        public void ModifyAttribute_IgnoresMissingAttributeType()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));
            
            modifiable
                .Invoking(obj => obj.CallMethod(
                    ModifyAttributeMethodName, AttributeType.DroneCount, Modification.AddFractionOfBase(1)))
                .Should()
                .NotThrow();
            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }

        [Fact]
        public void CreateModifiableInstance_ReturnsAModifiableInstance()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            modifiable.CallMethod(CreateModifiableInstanceMethodName).Should().BeAssignableTo(ModifiableType);
        }

        [Fact]
        public void CreateModifiableInstance_DoesNotCopyModifications()
        {
            var original = ConstructModifiable(ConstructTemplate(42, 10, ConstructWrappedInt(18)));

            original.CallMethod(
                ModifyAttributeMethodName, AttributeType.Damage, Modification.AddFractionOfBase(1));
            var newModifiable = original.CallMethod(CreateModifiableInstanceMethodName);
            
            newModifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntProperty)).Should().Be(42);
            newModifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
            newModifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(18);
        }
    }
}
