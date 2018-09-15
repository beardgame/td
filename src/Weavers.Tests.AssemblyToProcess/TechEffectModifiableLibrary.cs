using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Shared.TechEffects;

namespace Weavers.Tests.AssemblyToProcess
{
    public sealed class TechEffectModifiableLibrary : TechEffectModifiableLibrary<TechEffectModifiableLibrary>
    {
        public override IDictionary<Type, Type> GetInterfaceToTemplateMap()
        {
            var dict = new Dictionary<Type, Type>();
            dict.Add(typeof(ITechEffectDummy), typeof(TechEffectReference));
            return new ReadOnlyDictionary<Type, Type>(dict);
        }
    }
}
