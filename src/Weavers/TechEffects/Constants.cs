using System;
using Bearded.TD.Shared.TechEffects;

namespace Weavers.TechEffects
{
    static class Constants
    {
        internal static readonly Type Interface = typeof(IParametersTemplate);
        internal static readonly Type ModifiableAttribute = typeof(ModifiableAttribute);

        internal static string GetTemplateClassNameForInterface(string interfaceName)
            => getInterfaceBaseName(interfaceName) + "Template";
        internal static string GetParameterClassNameForInterface(string interfaceName)
            => getInterfaceBaseName(interfaceName) + "Modifiable";

        private static string getInterfaceBaseName(string interfaceName)
        {
            if (interfaceName[0] == 'I') interfaceName = interfaceName.Substring(1);
            if (interfaceName.EndsWith("Template")) interfaceName = interfaceName.Replace("Template", "");
            return interfaceName;
        }
    }
}
