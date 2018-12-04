using System;
using Bearded.TD.Shared.TechEffects;

namespace Weavers.TechEffects
{
    static class Constants
    {
        internal static readonly Type Interface = typeof(IParametersTemplate<>);
        internal static readonly Type ModifiableAttribute = typeof(ModifiableAttribute);
        internal static readonly Type ModifiableBase = typeof(ModifiableBase);
        internal static readonly Type AttributeWithModificationsType = typeof(AttributeWithModifications<>);

        // Can't nameof these because they are members of a generic class with annoying limitations :(
        internal static readonly string CreateModifiableInstanceMethod = "CreateModifiableInstance";
        internal static readonly string HasAttributeOfTypeMethod = "HasAttributeOfType";
        internal static readonly string ModifyAttributeMethod = "ModifyAttribute";

        // Can't nameof this one due to protectedness :(
        internal static readonly string ModifiableBaseInitializeMethod = "InitializeAttributes";

        internal static string GetTemplateClassNameForInterface(string interfaceName)
            => getInterfaceBaseName(interfaceName) + "Template";
        internal static string GetModifiableClassNameForInterface(string interfaceName)
            => getInterfaceBaseName(interfaceName) + "Modifiable";

        internal static readonly string TemplateFieldName = "template";

        private static string getInterfaceBaseName(string interfaceName)
        {
            if (interfaceName[0] == 'I') interfaceName = interfaceName.Substring(1);
            if (interfaceName.EndsWith("Template")) interfaceName = interfaceName.Replace("Template", "");
            return interfaceName;
        }
    }
}
