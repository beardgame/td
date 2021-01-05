using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Components
{
    static class ParametersTemplateLibrary
    {
        public static ImmutableDictionary<Type, Type> TemplateTypeByInterface { get; }
        private static ImmutableDictionary<Type, Type> modifiableTypeByInterface { get; }

        static ParametersTemplateLibrary()
        {
            TemplateTypeByInterface = Assembly.GetExecutingAssembly().GetTypes()
                .Select(t => (Type: t, Attribute: t.GetCustomAttribute<ParametersTemplateAttribute>(false)))
                .Where(t => t.Attribute != null)
                .ToImmutableDictionary(t => t.Attribute!.Interface, t => t.Type);
            modifiableTypeByInterface = Assembly.GetExecutingAssembly().GetTypes()
                .Select(t => (Type: t, Attribute: t.GetCustomAttribute<ParametersModifiableAttribute>(false)))
                .Where(t => t.Attribute != null)
                .ToImmutableDictionary(t => t.Attribute!.Interface, t => t.Type);
        }

        public static void TouchModifiableClasses()
        {
            foreach (var (_, type) in modifiableTypeByInterface)
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
        }
    }
}
