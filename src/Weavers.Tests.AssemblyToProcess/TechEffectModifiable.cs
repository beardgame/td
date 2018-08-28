using Bearded.TD.Shared.TechEffects;

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

        public TechEffectReference(int intProperty)
        {
            IntProperty = intProperty;
        }
    }
}
