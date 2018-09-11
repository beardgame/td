using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using TypeSystem = Fody.TypeSystem;

namespace Weavers.TechEffects
{
    sealed class ImplementationWeaver
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly TypeSystem typeSystem;
        private readonly ILogger logger;
        private readonly ReferenceFinder referenceFinder;

        private readonly TypeReference techEffectInterface;

        internal ImplementationWeaver(
            ModuleDefinition moduleDefinition,
            TypeSystem typeSystem,
            ILogger logger,
            ReferenceFinder referenceFinder)
        {
            this.moduleDefinition = moduleDefinition;
            this.typeSystem = typeSystem;
            this.logger = logger;
            this.referenceFinder = referenceFinder;

            techEffectInterface = this.referenceFinder.GetTypeReference(Constants.Interface);
        }

        public void Execute()
        {
            var typesToImplement = moduleDefinition.Types
                .Where(type =>
                    type != null && type.IsInterface && type.ImplementsInterface(Constants.Interface))
                .ToList();

            foreach (var @interface in typesToImplement)
            {
                foreach (var implementation in createImplementations(@interface))
                {
                    moduleDefinition.Types.Add(implementation);
                }
            }
        }

        private IEnumerable<TypeDefinition> createImplementations(TypeDefinition interfaceToImplement)
        {
            logger.LogInfo($"Weaving implementations for {interfaceToImplement}.");

            var properties = interfaceToImplement.Properties
                .Where(p => p.TryGetCustomAttribute(Constants.ModifiableAttribute, out _))
                .ToList();

            yield return createTemplateImplementation(interfaceToImplement, properties);
        }

        private TypeDefinition createTemplateImplementation(
            TypeReference interfaceToImplement,
            IReadOnlyCollection<PropertyDefinition> properties)
        {
            var templateType = new TypeDefinition(
                interfaceToImplement.Namespace,
                Constants.GetTemplateClassNameForInterface(interfaceToImplement.Name),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                typeSystem.ObjectReference);
            
            templateType.AddInterfaceImplementation(interfaceToImplement);
            templateType.AddInterfaceImplementation(techEffectInterface);
            
            var fieldsByProperty = addTemplateConstructor(templateType, properties);

            foreach (var entry in fieldsByProperty)
            {
                addTemplateProperty(templateType, entry.Key, entry.Value);
            }

            return templateType;
        }

        private Dictionary<PropertyDefinition, FieldReference> addTemplateConstructor(TypeDefinition type, IReadOnlyCollection<PropertyDefinition> properties)
        {
            var method = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                typeSystem.VoidReference);

            method.AddCustomAttribute(typeof(JsonConstructorAttribute), referenceFinder);

            var propertyFields = new List<FieldDefinition>();
            var fieldsByProperty = new Dictionary<PropertyDefinition, FieldReference>();

            foreach (var property in properties)
            {
                method.Parameters.Add(
                    new ParameterDefinition(
                        property.Name.ToCamelCase(), ParameterAttributes.None, property.PropertyType));

                var fieldDef = new FieldDefinition(
                    property.Name.ToCamelCase(),
                    FieldAttributes.Private | FieldAttributes.InitOnly,
                    property.PropertyType);

                type.Fields.Add(fieldDef);
                propertyFields.Add(fieldDef);
                fieldsByProperty.Add(property, fieldDef);
            }

            var objectConstructor =
                referenceFinder.GetMethodReference(typeSystem.ObjectDefinition, m => m.IsConstructor);
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, objectConstructor);

            for (var i = 0; i < properties.Count; i++)
            {
                processor.Emit(OpCodes.Ldarg_0);
                processor.Emit(OpCodes.Ldarg, i + 1);
                processor.Emit(OpCodes.Stfld, propertyFields[i]);
            }

            processor.Emit(OpCodes.Ret);
            type.Methods.Add(method);

            return fieldsByProperty;
        }

        private void addTemplateProperty(TypeDefinition type, PropertyDefinition propertyBase, FieldReference fieldReference)
        {
            var getMethodBase = moduleDefinition.ImportReference(propertyBase.GetMethod).Resolve();

            var propertyImpl =
                new PropertyDefinition(propertyBase.Name, PropertyAttributes.None, propertyBase.PropertyType)
                {
                    GetMethod = new MethodDefinition(
                        getMethodBase.Name,
                        MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig
                            | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                        getMethodBase.ReturnType)
                };

            var getMethodImpl = propertyImpl.GetMethod;
            getMethodImpl.SemanticsAttributes = getMethodBase.SemanticsAttributes;
            getMethodImpl.Body = new MethodBody(getMethodImpl);

            var processor = getMethodImpl.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, fieldReference);
            processor.Emit(OpCodes.Ret);
            type.Properties.Add(propertyImpl);
            type.Methods.Add(getMethodImpl);
        }
    }
}
