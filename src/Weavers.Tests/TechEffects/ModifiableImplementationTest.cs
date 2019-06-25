using System;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
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
        public void AddModification_ModifiesSimpleValue()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            modifiable.CallMethod(
                AddModificationMethodName, AttributeType.Damage, Modification.AddFractionOfBase(1));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(20);
        }

        [Fact]
        public void AddModification_ModifiesWrappedValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, null, ConstructWrappedInt(18)));
            
            modifiable.CallMethod(
                AddModificationMethodName, AttributeType.Cooldown, Modification.AddFractionOfBase(1));
            
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(36);
        }

        [Fact]
        public void AddModification_IgnoresMissingAttributeType()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));
            
            modifiable
                .Invoking(obj => obj.CallMethod(
                    AddModificationMethodName, AttributeType.DroneCount, Modification.AddFractionOfBase(1)))
                .Should()
                .NotThrow();
            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }

        [Fact]
        public void AddModificationWithId_ModifiesSimpleValue()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            modifiable.CallMethod(
                AddModificationWithIdMethodName, AttributeType.Damage,
                new ModificationWithId(new Id<Modification>(1), Modification.AddFractionOfBase(1)));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(20);
        }

        [Fact]
        public void AddModificationWithId_ModifiesWrappedValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, null, ConstructWrappedInt(18)));
            
            modifiable.CallMethod(
                AddModificationWithIdMethodName, AttributeType.Cooldown,
                new ModificationWithId(new Id<Modification>(1), Modification.AddFractionOfBase(1)));
            
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(36);
        }

        [Fact]
        public void AddModificationWithId_IgnoresMissingAttributeType()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));
            
            modifiable
                .Invoking(obj =>
                    obj.CallMethod(
                        AddModificationWithIdMethodName, AttributeType.DroneCount,
                        new ModificationWithId(new Id<Modification>(1), Modification.AddFractionOfBase(1))))
                .Should()
                .NotThrow();
            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }

        [Fact]
        public void UpdateModification_ModifiesSimpleValue()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            var id = new Id<Modification>(1);
            modifiable.CallMethod(
                AddModificationWithIdMethodName, AttributeType.Damage,
                new ModificationWithId(id, Modification.AddFractionOfBase(1)));
            modifiable.CallMethod(
                UpdateModificationMethodName, AttributeType.Damage, id, Modification.AddFractionOfBase(2));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(30);
        }

        [Fact]
        public void UpdateModification_ModifiesWrappedValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, null, ConstructWrappedInt(18)));
            
            var id = new Id<Modification>(1);
            modifiable.CallMethod(
                AddModificationWithIdMethodName, AttributeType.Cooldown,
                new ModificationWithId(id, Modification.AddFractionOfBase(1)));
            modifiable.CallMethod(
                UpdateModificationMethodName, AttributeType.Cooldown, id, Modification.AddFractionOfBase(2));
            
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(54);
        }

        [Fact]
        public void UpdateModification_IgnoresMissingAttributeType()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            modifiable
                .Invoking(obj =>
                    obj.CallMethod(
                        UpdateModificationMethodName, AttributeType.DroneCount, new Id<Modification>(1),
                        Modification.AddFractionOfBase(1)))
                .Should()
                .NotThrow();
            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }

        [Fact]
        public void UpdateModification_ThrowsIfModificationNotFound()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            modifiable
                .Invoking(obj =>
                    obj.CallMethod(
                        UpdateModificationMethodName, AttributeType.Damage, new Id<Modification>(1),
                        Modification.AddFractionOfBase(1)))
                .Should()
                .Throw<Exception>() // Reflection wrapper
                .WithInnerException<InvalidOperationException>();
        }

        [Fact]
        public void RemoveModification_RestoresModifiedSimpleValue()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            var id = new Id<Modification>(1);
            modifiable.CallMethod(
                AddModificationWithIdMethodName, AttributeType.Damage,
                new ModificationWithId(id, Modification.AddFractionOfBase(1)));
            modifiable.CallMethod(RemoveModificationMethodName, AttributeType.Damage, id);

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(10);
        }

        [Fact]
        public void RemoveModification_RestoresModifiedWrappedValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, null, ConstructWrappedInt(18)));
            
            var id = new Id<Modification>(1);
            modifiable.CallMethod(
                AddModificationWithIdMethodName, AttributeType.Cooldown,
                new ModificationWithId(id, Modification.AddFractionOfBase(1)));
            modifiable.CallMethod(RemoveModificationMethodName, AttributeType.Cooldown, id);
            
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(18);
        }

        [Fact]
        public void RemoveModification_IgnoresMissingAttributeType()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));
            
            modifiable
                .Invoking(obj =>
                    obj.CallMethod(
                        RemoveModificationMethodName, AttributeType.DroneCount, new Id<Modification>(1)))
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
                AddModificationMethodName, AttributeType.Damage, Modification.AddFractionOfBase(1));
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
