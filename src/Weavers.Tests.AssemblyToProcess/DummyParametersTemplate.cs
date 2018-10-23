using System.Collections.Immutable;
using Bearded.TD.Shared.TechEffects;
using Newtonsoft.Json;

namespace Weavers.Tests.AssemblyToProcess
{
    public interface IDummyParametersTemplate : IParametersTemplate<IDummyParametersTemplate>
    {
        [Modifiable]
        int IntProperty { get; }
        
        [Modifiable(10, Type = AttributeType.DamagePerUnit)]
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

        public bool ModifyAttribute(AttributeType type, Modification modification)
        {
            throw new System.InvalidOperationException("Cannot modify attributes on immutable template.");
        }
    }

    public sealed class DummyParametersModifiableReference : IDummyParametersTemplate
    {
        private readonly IDummyParametersTemplate template;

        private readonly AttributeWithModifications<int> intProperty;
        private readonly AttributeWithModifications<int> intPropertyWithDefault;
        private readonly AttributeWithModifications<WrappedInt> wrappedIntProperty;

        public int IntProperty => intProperty.Value;
        public int IntPropertyWithDefault => intPropertyWithDefault.Value;
        public WrappedInt WrappedIntProperty => wrappedIntProperty.Value;

        private readonly ImmutableDictionary<AttributeType, ImmutableList<IAttributeWithModifications>> attributesByType;

        public DummyParametersModifiableReference(IDummyParametersTemplate template)
        {
            this.template = template;
            intProperty = new AttributeWithModifications<int>(template.IntProperty, i => (int) i);
            intPropertyWithDefault = new AttributeWithModifications<int>(template.IntPropertyWithDefault, i => (int) i);
            wrappedIntProperty = new AttributeWithModifications<WrappedInt>(template.WrappedIntProperty.Val, i => new WrappedInt((int) i));

            var dictBuilder = ImmutableDictionary
                .CreateBuilder<AttributeType, ImmutableList<IAttributeWithModifications>>();
            dictBuilder.Add(AttributeType.DamagePerUnit, ImmutableList.Create<IAttributeWithModifications>(intPropertyWithDefault));
            dictBuilder.Add(AttributeType.Cooldown, ImmutableList.Create<IAttributeWithModifications>(wrappedIntProperty));
            attributesByType = dictBuilder.ToImmutable();
        }

        public IDummyParametersTemplate CreateModifiableInstance() => new DummyParametersModifiableReference(template);

        public bool ModifyAttribute(AttributeType type, Modification modification)
        {
            if (!attributesByType.TryGetValue(type, out var list)) return false;

            foreach (var attribute in list)
            {
                attribute.AddModification(modification);
            }

            return true;
        }
    }

    public struct WrappedInt
    {
        public int Val { get; }

        public WrappedInt(int val) => Val = val;
    }
}
