using System;
using Bearded.TD.Shared.TechEffects;

namespace Weavers.TechEffects
{
    static class Constants
    {
        internal static readonly Type Interface = typeof(ITechEffectModifiable);
        internal static readonly Type ModifiableAttribute = typeof(ModifiableAttribute);

        internal static string GetTemplateClassNameForInterface(string interfaceName)
            => getInterfaceBaseName(interfaceName) + "Template";
        internal static string GetParameterClassNameForInterface(string interfaceName)
            => getInterfaceBaseName(interfaceName) + "Parameter";

        private static string getInterfaceBaseName(string interfaceName) =>
            interfaceName[0] == 'I' ? interfaceName.Substring(1) : interfaceName;
    }
}
