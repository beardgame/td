using System;
using System.Collections.Generic;

namespace Bearded.TD.Shared.TechEffects
{
    public interface IParametersTemplateLibrary
    {
        IDictionary<Type, Type> GetInterfaceToTemplateMap();
    }

    public class ParametersTemplateLibrary<T> : IParametersTemplateLibrary
        where T : ParametersTemplateLibrary<T>, new()
    {
        public static T Instance { get; } = new T();

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        // The method is overridden in the generated subtypes.
        public virtual IDictionary<Type, Type> GetInterfaceToTemplateMap() => throw new InvalidOperationException();
    }
}
