using System;
using System.Collections.Generic;

namespace Bearded.TD.Shared.TechEffects
{
    public interface ITechEffectModifiableLibrary
    {
        IDictionary<Type, Type> GetInterfaceToTemplateMap();
    }

    public class TechEffectModifiableLibrary<T> : ITechEffectModifiableLibrary
        where T : TechEffectModifiableLibrary<T>, new()
    {
        public static T Instance { get; } = new T();

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        // The method is overridden in the generated subtypes.
        public virtual IDictionary<Type, Type> GetInterfaceToTemplateMap() => throw new InvalidOperationException();
    }
}
