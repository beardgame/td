using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TypeSystem = Fody.TypeSystem;

namespace Weavers.TechEffects
{
    sealed class ModifiableImplementationWeaver : BaseImplementationWeaver
    {
        private readonly TypeReference baseClassType;

        public ModifiableImplementationWeaver(
                ModuleDefinition moduleDefinition,
                TypeSystem typeSystem,
                ILogger logger,
                ReferenceFinder referenceFinder)
            : base(moduleDefinition, typeSystem, logger, referenceFinder)
        {
            baseClassType = ReferenceFinder.GetTypeReference(Constants.ModifiableBase);
        }

        public TypeDefinition WeaveImplementation(
            TypeReference interfaceToImplement,
            IReadOnlyCollection<PropertyDefinition> properties)
        {
            var (modifiableType, genericParameterInterface) = PrepareImplementation(
                interfaceToImplement,
                Constants.GetModifiableClassNameForInterface(interfaceToImplement.Name),
                baseClassType);

            var fieldsByProperty = addConstructor(modifiableType, interfaceToImplement, properties);

            foreach (var entry in fieldsByProperty)
            {
                addProperty(modifiableType, entry.Key, entry.Value);
            }

            addCreateModifiableInstanceMethod(modifiableType, genericParameterInterface);

            return modifiableType;
        }
        
        private Dictionary<PropertyDefinition, FieldReference> addConstructor(
            TypeDefinition type, TypeReference interfaceToImplement, IReadOnlyCollection<PropertyDefinition> properties)
        {
            var method = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName
                | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                TypeSystem.VoidReference);

            var templateField = new FieldDefinition(
                Constants.TemplateFieldName,
                FieldAttributes.Private | FieldAttributes.InitOnly,
                interfaceToImplement);
            type.Fields.Add(templateField);
            
            var fieldsByProperty = new Dictionary<PropertyDefinition, FieldReference>();
            var fieldsWithType = new List<(FieldReference, AttributeType)>();

            foreach (var property in properties)
            {
                property.TryGetCustomAttribute(typeof(ModifiableAttribute), out var modifiableAttribute);
                var attributeParameters = modifiableAttribute.Constructor.Resolve().Parameters;

                var attributeWithModificationsType =
                    ReferenceFinder.GetTypeReference(Constants.AttributeWithModificationsType);
                var fieldType = attributeWithModificationsType.MakeGenericInstanceType(property.PropertyType);
            }

            var baseConstructor = ReferenceFinder.GetConstructorReference(baseClassType);

            var processor = method.Body.GetILProcessor();

            // call base constructor
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, baseConstructor);

            // set template field
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Stfld, templateField);

            // create modifiable wrappers for all properties
            // TODO

            // call base dictionary initialization
            // TODO

            processor.Emit(OpCodes.Ret);
            type.Methods.Add(method);

            return fieldsByProperty;
        }

        private void addProperty(
            TypeDefinition type, PropertyDefinition propertyBase, FieldReference fieldReference)
        {
            var propertyImpl = MethodHelpers.CreatePropertyImplementation(ModuleDefinition, propertyBase);
            var getMethodImpl = propertyImpl.GetMethod;

            var fieldType = ModuleDefinition.ImportReference(fieldReference.FieldType).Resolve();
            var valueProperty = fieldType.Properties.First(p => p.Name == nameof(AttributeWithModifications<int>.Value));

            var processor = getMethodImpl.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, fieldReference);
            processor.Emit(OpCodes.Call, valueProperty.GetMethod);
            processor.Emit(OpCodes.Ret);
            type.Properties.Add(propertyImpl);
            type.Methods.Add(getMethodImpl);
        }

        private void addCreateModifiableInstanceMethod(TypeDefinition type, GenericInstanceType genericParameterInterface)
        {
            var field = type.Fields.First(f => f.Name == Constants.TemplateFieldName);
            //var field = ReferenceFinder.GetFieldReference(type, Constants.TemplateFieldName);
            ImplementCreateModifiableInstanceMethod(type, genericParameterInterface, type, field);
        }
    }
}
