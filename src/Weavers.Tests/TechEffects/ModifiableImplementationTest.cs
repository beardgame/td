using System;
using Bearded.TD.Shared.TechEffects;
using FluentAssertions;
using Weavers.Tests.AssemblyToProcess;
using Xunit;

namespace Weavers.Tests.TechEffects
{
    public sealed class ModifiableImplementationTest : ImplementationTestBase
    {
        private const string modifyAttributeMethodName = "ModifyAttribute";
        
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
            var modifiable = ConstructModifiable(
                ConstructTemplate(
                    0,
                    null,
                    // ReSharper disable once PossibleNullReferenceException
                    WrappedIntType.GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 18 })));
            
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(18);
        }
        
        [Fact]
        public void ModifiesValues()
        {
            var modifiable = ConstructModifiable(ConstructTemplate(0, 10, null));

            modifiable.CallMethod(
                modifyAttributeMethodName, AttributeType.Damage, Modification.AddFractionOfBase(1));

            modifiable.GetPropertyValue<int>(nameof(IDummyParametersTemplate.IntPropertyWithDefault)).Should().Be(20);
        }
        
        [Fact]
        public void ModifiesWrappedValues()
        {
            var modifiable = ConstructModifiable(
                ConstructTemplate(
                    0,
                    null,
                    // ReSharper disable once PossibleNullReferenceException
                    WrappedIntType.GetConstructor(new[] { typeof(int) }).Invoke(new object[] { 18 })));
            
            modifiable.CallMethod(
                modifyAttributeMethodName, AttributeType.Cooldown, Modification.AddFractionOfBase(1));
            
            modifiable
                .GetPropertyValue<object>(nameof(IDummyParametersTemplate.WrappedIntProperty))
                .GetPropertyValue<int>(nameof(WrappedInt.Val))
                .Should().Be(36);
        }
    }
}
