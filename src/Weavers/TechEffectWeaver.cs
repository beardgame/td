using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
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
                if (type == null || !type.IsInterface || !type.ImplementsInterface(TechEffects.Interface))
                {
                    continue;
                }

                toAdd.EnqueueAll(createImplementations(type));
            }

            foreach (var type in toAdd) ModuleDefinition.Types.Add(type);
        }

        private IEnumerable<TypeDefinition> createImplementations(TypeDefinition interfaceDef)
        {
            LogInfo($"Weaving implementations for {interfaceDef}.");

            var properties = interfaceDef.Properties
                .Where(p => p.TryGetCustomAttribute(TechEffects.ModifiableAttribute, out _))
                .ToList();

            yield return createTemplateImplementation(interfaceDef, properties);
        }

        private TypeDefinition createTemplateImplementation(
            TypeReference interfaceRef, IReadOnlyCollection<PropertyDefinition> properties)
        {
            var templateType = new TypeDefinition(
                interfaceRef.Namespace,
                getInterfaceBaseName(interfaceRef.Name) + TechEffects.TemplateSuffix,
                TypeAttributes.Public,
                TypeSystem.ObjectReference);
            var templateImplementation = new InterfaceImplementation(interfaceRef);
            templateType.Interfaces.Add(templateImplementation);
            
            addTemplateConstructor(templateType, properties);

            foreach (var property in properties)
            {
                addTemplateProperty(templateType, property);
            }

            return templateType;
        }

        private void addTemplateConstructor(TypeDefinition type, IEnumerable<PropertyDefinition> properties)
        {
            var method = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                TypeSystem.VoidReference);

            foreach (var property in properties)
            {
                method.Parameters.Add(new ParameterDefinition(property.Name.ToCamelCase(), ParameterAttributes.None, property.PropertyType));
            }

            var objectConstructor = ModuleDefinition.ImportReference(TypeSystem.ObjectDefinition.GetConstructors().First());
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, objectConstructor);
            processor.Emit(OpCodes.Ret);
            type.Methods.Add(method);
        }

        private void addTemplateProperty(TypeDefinition type, PropertyDefinition propertyBase)
        {
            var propertyImpl =
                new PropertyDefinition(propertyBase.Name, PropertyAttributes.None, propertyBase.PropertyType)
                {
                    GetMethod = new MethodDefinition(
                        propertyBase.GetMethod.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName,
                        propertyBase.PropertyType)
                };

            propertyBase.GetMethod.Overrides.Add(propertyImpl.GetMethod);

            var processor = propertyImpl.GetMethod.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldc_I4_0);
            processor.Emit(OpCodes.Ret);
            type.Properties.Add(propertyImpl);
        }

        private static string getInterfaceBaseName(string interfaceName) =>
            interfaceName[0] == 'I' ? interfaceName.Substring(1) : interfaceName;

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield break;
        }
    }
}
