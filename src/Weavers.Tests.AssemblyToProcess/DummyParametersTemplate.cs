using System.Collections.Generic;
using Bearded.TD.Shared.TechEffects;
using Newtonsoft.Json;

namespace Weavers.Tests.AssemblyToProcess
{
    public interface IDummyParametersTemplate : IParametersTemplate<IDummyParametersTemplate>
    {
        [Modifiable]
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

        public IDummyParametersTemplate CreateModifiableInstance() => new DummyParametersModifiableReference(this);

        public bool HasAttributeOfType(AttributeType type)
        {
            throw new System.InvalidOperationException("Cannot check attributes on immutable template.");
        }

        public bool ModifyAttribute(AttributeType type, Modification modification)
        {
            throw new System.InvalidOperationException("Cannot modify attributes on immutable template.");
        }
    }

    public sealed class DummyParametersModifiableReference : ModifiableBase, IDummyParametersTemplate
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

            InitializeAttributes(new List<KeyValuePair<AttributeType, IAttributeWithModifications>>
            {
                new KeyValuePair<AttributeType, IAttributeWithModifications>(AttributeType.Damage, intPropertyWithDefault),
                new KeyValuePair<AttributeType, IAttributeWithModifications>(AttributeType.Cooldown, wrappedIntProperty)
            });
        }

        public IDummyParametersTemplate CreateModifiableInstance() => new DummyParametersModifiableReference(template);
    }

    public struct WrappedInt
    {
        public int Val { get; }

        public WrappedInt(int val) => Val = val;
    }
}
