using System;
using Bearded.TD.Shared.TechEffects;

namespace Weavers.TechEffects
{
    static class Constants
    {
        internal static readonly Type Interface = typeof(IParametersTemplate<>);
        internal static readonly Type ModifiableAttribute = typeof(ModifiableAttribute);
        internal static readonly Type ModifiableBase = typeof(ModifiableBase<>);
        internal static readonly Type AttributeWithModificationsType = typeof(AttributeWithModifications<>);

        // Can't nameof these because they are members of a generic class with annoying limitations :(
        internal const string CreateModifiableInstanceMethod = "CreateModifiableInstance";
        internal const string HasAttributeOfTypeMethod = "HasAttributeOfType";
        internal const string AddModificationMethod = "AddModification";
        internal const string AddModificationWithIdMethod = "AddModificationWithId";
        internal const string UpdateModificationMethod = "UpdateModification";
        internal const string RemoveModificationMethod = "RemoveModification";
        internal const string AttributeIsKnownMethod = "AttributeIsKnown";
        internal const string ToWrappedMethod = "ToWrapped";
        internal const string ToRawMethod = "ToRaw";

        internal static readonly string[] ModificationMethods = new[]
        {
            AddModificationMethod, AddModificationWithIdMethod, UpdateModificationMethod, RemoveModificationMethod
        };

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
