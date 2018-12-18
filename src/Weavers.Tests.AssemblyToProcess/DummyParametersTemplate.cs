using System;
using System.Collections.Generic;
using Bearded.TD.Shared.TechEffects;
using Newtonsoft.Json;

namespace Weavers.Tests.AssemblyToProcess
{
    public interface IDummyParametersTemplate : IParametersTemplate<IDummyParametersTemplate>
    {
        int IntProperty { get; }
        
        [Modifiable(10, Type = AttributeType.Damage)]
        int IntPropertyWithDefault { get; }
        
        [Modifiable(10, Type = AttributeType.Cooldown)]
        WrappedInt WrappedIntProperty { get; }
    }
    
    public sealed class DummyParametersReference : IDummyParametersTemplate
    {
        public int IntProperty { get; }
        public int IntPropertyWithDefault { get; }
        public WrappedInt WrappedIntProperty { get; }

        [JsonConstructor]
        public DummyParametersReference(int intProperty, int? intPropertyWithDefault, WrappedInt? wrappedInt)
        {
            IntProperty = intProperty;
            IntPropertyWithDefault = intPropertyWithDefault.GetValueOrDefault(10);
            WrappedIntProperty = wrappedInt.GetValueOrDefault(new WrappedInt(10));
        }

        public bool HasAttributeOfType(AttributeType type) =>
            DummyParametersModifiableReference.AttributeIsKnown(type);
        
        public bool ModifyAttribute(AttributeType type, Modification modification)
        {
            throw new System.InvalidOperationException("Cannot modify attributes on immutable template.");
        }

        public IDummyParametersTemplate CreateModifiableInstance() => new DummyParametersModifiableReference(this);
    }

    public sealed class DummyParametersModifiableReference : ModifiableBase<DummyParametersModifiableReference>, IDummyParametersTemplate
    {
        private readonly IDummyParametersTemplate template;

        private readonly AttributeWithModifications<int> intPropertyWithDefault;
        private readonly AttributeWithModifications<WrappedInt> wrappedIntProperty;

        public int IntProperty => template.IntProperty;
        public int IntPropertyWithDefault => intPropertyWithDefault.Value;
        public WrappedInt WrappedIntProperty => wrappedIntProperty.Value;

        public DummyParametersModifiableReference(IDummyParametersTemplate template)
        {
            this.template = template;

            intPropertyWithDefault = new AttributeWithModifications<int>(template.IntPropertyWithDefault, i => (int) i);
            wrappedIntProperty = new AttributeWithModifications<WrappedInt>(template.WrappedIntProperty.Val, i => new WrappedInt((int) i));
        }

        public IDummyParametersTemplate CreateModifiableInstance() => new DummyParametersModifiableReference(template);
        
        static DummyParametersModifiableReference()
        {
            InitializeAttributes(
                new List<KeyValuePair<AttributeType,
                    Func<DummyParametersModifiableReference, IAttributeWithModifications>>>
                {
                    new KeyValuePair<AttributeType,
                        Func<DummyParametersModifiableReference, IAttributeWithModifications>>(
                        AttributeType.Damage, getIntPropertyWithDefault
                    ),
                    new KeyValuePair<AttributeType,
                        Func<DummyParametersModifiableReference, IAttributeWithModifications>>(
                        AttributeType.Cooldown, getWrappedIntProperty
                    )
                }
            );
        }

        private static IAttributeWithModifications getIntPropertyWithDefault(DummyParametersModifiableReference instance)
            => instance.intPropertyWithDefault;
            
        private static IAttributeWithModifications getWrappedIntProperty(DummyParametersModifiableReference instance)
            => instance.intPropertyWithDefault;
    }

    public struct WrappedInt
    {
        public int Val { get; }

        public WrappedInt(int val) => Val = val;
    }
}
