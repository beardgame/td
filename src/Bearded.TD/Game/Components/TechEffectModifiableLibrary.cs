using System;
using System.Collections.Generic;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Components
{
    sealed class TechEffectModifiableLibrary : TechEffectModifiableLibrary<TechEffectModifiableLibrary>
    {
        public override IDictionary<Type, Type> GetInterfaceToTemplateMap() => throw new InvalidOperationException();
    }
}
