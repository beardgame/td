using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using static Weavers.Constants;

namespace Weavers
{
    public sealed class TechEffectWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            var allTypes = ModuleDefinition.Types;

            var toAdd = new Queue<TypeDefinition>();

            foreach (var type in allTypes)
            {
                if (type == null || !type.IsInterface || implementsInterface(type, TechEffects.Interface))
                {
                    continue;
                }

                toAdd.EnqueueAll(injectImplementations(type));
            }

            foreach (var type in toAdd) ModuleDefinition.Types.Add(type);
        }

        private IEnumerable<TypeDefinition> injectImplementations(TypeReference interfaceRef)
        {
            LogInfo($"Weaving implementations for {interfaceRef}.");

            var interfaceBaseName = getInterfaceBaseName(interfaceRef.Name);

            var templateType = new TypeDefinition(
                interfaceRef.Namespace,
                interfaceBaseName + TechEffects.TemplateSuffix,
                TypeAttributes.Public,
                TypeSystem.ObjectReference);
            var templateImplementation = new InterfaceImplementation(interfaceRef);
            templateType.Interfaces.Add(templateImplementation);

            yield return templateType;
        }

        private static bool implementsInterface(TypeDefinition type, string interfaceName)
        {
            return type.HasInterfaces
                && type.Interfaces.Any(interfaceImplementation =>
                    interfaceImplementation.InterfaceType.Name == interfaceName);
        }

        private static string getInterfaceBaseName(string interfaceName) =>
            interfaceName[0] == 'I' ? interfaceName.Substring(1) : interfaceName;

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield break;
        }
    }
}
