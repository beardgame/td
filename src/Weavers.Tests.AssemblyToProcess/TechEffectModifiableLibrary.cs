using System;
using System.Collections.Generic;
using Bearded.TD.Shared.TechEffects;

namespace Weavers.Tests.AssemblyToProcess
{
    public sealed class TechEffectModifiableLibrary : TechEffectModifiableLibrary<TechEffectModifiableLibrary>
    {
        public override IDictionary<Type, Type> GetInterfaceToTemplateMap() => throw new InvalidOperationException();
    }
}
