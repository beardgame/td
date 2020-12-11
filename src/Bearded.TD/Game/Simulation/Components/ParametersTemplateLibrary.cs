using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Components
{
    static class ParametersTemplateLibrary
    {
        public static ImmutableDictionary<Type, Type> TemplateTypeByInterface { get; }

        static ParametersTemplateLibrary()
        {
            TemplateTypeByInterface = Assembly.GetExecutingAssembly().GetTypes()
                .Select(t => (Type: t, Attribute: t.GetCustomAttribute<ParametersTemplateAttribute>(false)))
                .Where(t => t.Attribute != null)
                .ToImmutableDictionary(t => t.Attribute!.Interface, t => t.Type);
        }
    }
}
