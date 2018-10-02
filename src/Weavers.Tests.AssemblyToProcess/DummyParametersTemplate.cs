using Bearded.TD.Shared.TechEffects;
using Newtonsoft.Json;

namespace Weavers.Tests.AssemblyToProcess
{
    public interface IDummyParametersTemplate : IParametersTemplate<IDummyParametersTemplate>
    {
        [Modifiable]
        int IntProperty { get; }
        
        [Modifiable(10)]
        int IntPropertyWithDefault { get; }
        
        [Modifiable(10)]
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

        public IDummyParametersTemplate CreateModifiableInstance()
        {
            return null;
        }

        public void ModifyAttribute(ModificationType type)
        {
            throw new System.InvalidOperationException("Cannot modify attributes on immutable template.");
        }
    }

    public struct WrappedInt
    {
        public int Val { get; }

        public WrappedInt(int val) => Val = val;
    }
}
