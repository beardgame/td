using Bearded.TD.Shared.TechEffects;
using Newtonsoft.Json;

namespace Weavers.Tests.AssemblyToProcess
{
    public interface ITechEffectDummy : ITechEffectModifiable
    {
        [Modifiable]
        int IntProperty { get; }
    }

    public sealed class TechEffectReference : ITechEffectDummy
    {
        public int IntProperty { get; }

        [JsonConstructor]
        public TechEffectReference(int intProperty)
        {
            IntProperty = intProperty;
        }
    }
}
