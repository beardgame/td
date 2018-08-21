using System;

namespace Weavers.Tests.AssemblyToProcess
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ModifiableAttribute : Attribute { }

    public interface ITechEffectModifiable {}

    public interface ITechEffectDummy : ITechEffectModifiable
    {
        [Modifiable]
        int IntProperty { get; }
    }
}
