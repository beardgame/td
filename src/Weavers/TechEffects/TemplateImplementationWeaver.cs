using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using Newtonsoft.Json;
using TypeSystem = Fody.TypeSystem;

namespace Weavers.TechEffects
{
    sealed class TemplateImplementationWeaver : BaseImplementationWeaver
    {
        private readonly TypeReference nullableReference;

        public TemplateImplementationWeaver(
                ModuleDefinition moduleDefinition, TypeSystem typeSystem, ILogger logger, ReferenceFinder referenceFinder)
            : base(moduleDefinition, typeSystem, logger, referenceFinder)
        {
            nullableReference = ReferenceFinder.GetTypeReference(typeof(Nullable<>));
        }

        public TypeDefinition WeaveImplementation(
            TypeReference interfaceToImplement,
            IReadOnlyCollection<PropertyDefinition> properties,
            TypeReference modifiableType)
        {
            var (templateType, genericParameterInterface) = PrepareImplementation(
                interfaceToImplement,
                Constants.GetTemplateClassNameForInterface(interfaceToImplement.Name),
                TypeSystem.ObjectReference);
            
            var fieldsByProperty = addConstructor(templateType, properties);

            foreach (var entry in fieldsByProperty)
            {
                addProperty(templateType, entry.Key, entry.Value);
            }

            addCreateModifiableInstanceMethod(templateType, genericParameterInterface, modifiableType);
            addHasAttributeOfTypeMethod(templateType, genericParameterInterface, modifiableType);
            addModifyAttributeMethod(templateType, genericParameterInterface);

            return templateType;
        }

        private Dictionary<PropertyDefinition, FieldReference> addConstructor(
            TypeDefinition type, IReadOnlyCollection<PropertyDefinition> properties)
        {
            var method = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName
                | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                TypeSystem.VoidReference);

            method.AddCustomAttribute(typeof(JsonConstructorAttribute), ReferenceFinder);

            var propertyFields = new List<(FieldDefinition, List<Action<ILProcessor>>)>();
            var fieldsByProperty = new Dictionary<PropertyDefinition, FieldReference>();

            foreach (var property in properties)
            {
                var parameterType = property.PropertyType;
                var attributeParameters = new Collection<ParameterDefinition>();

                if (property.TryGetCustomAttribute(typeof(ModifiableAttribute), out var modifiableAttribute))
                {
                    attributeParameters = modifiableAttribute.Constructor.Resolve().Parameters;
                    parameterType = attributeParameters.Count == 0
                        ? property.PropertyType
                        : nullableReference.MakeGenericInstanceType(property.PropertyType);
                }
                
                method.Parameters.Add(
                    new ParameterDefinition(
                        property.Name.ToCamelCase(), ParameterAttributes.None, parameterType));

                var fieldDef = new FieldDefinition(
                    property.Name.ToCamelCase(),
                    FieldAttributes.Private | FieldAttributes.InitOnly,
                    property.PropertyType);

                type.Fields.Add(fieldDef);

                var processorCommands = attributeParameters.Count > 0
                    ? getConstructorCommandsForProperty(property, modifiableAttribute)
                    : new List<Action<ILProcessor>>();

                propertyFields.Add((fieldDef, processorCommands));
                fieldsByProperty.Add(property, fieldDef);
            }

            var objectConstructor = ReferenceFinder.GetConstructorReference(TypeSystem.ObjectDefinition);
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, objectConstructor);

            for (var i = 0; i < properties.Count; i++)
            {
                processor.Emit(OpCodes.Ldarg_0);

                if (propertyFields[i].Item2.Count > 0)
                {
                    processor.Emit(OpCodes.Ldarga, i + 1);
                    foreach (var cmd in propertyFields[i].Item2)
                        cmd(processor);
                }
                else
                {
                    processor.Emit(OpCodes.Ldarg, i + 1);
                }

                processor.Emit(OpCodes.Stfld, propertyFields[i].Item1);
            }

            processor.Emit(OpCodes.Ret);
            type.Methods.Add(method);

            return fieldsByProperty;
        }

        private List<Action<ILProcessor>> getConstructorCommandsForProperty(
            PropertyReference property, ICustomAttribute modifiableAttribute)
        {
            var processorCommands = new List<Action<ILProcessor>>();
            var defaultValue = (CustomAttributeArgument) modifiableAttribute.ConstructorArguments[0].Value;

            processorCommands.Add(p => ILHelpers.EmitLd(p, defaultValue.Type, defaultValue.Value));

            if (!property.PropertyType.IsPrimitive)
            {
                var expectedConstructorParamType = (Type) modifiableAttribute.Properties
                    .FirstOrDefault(p => p.Name == nameof(ModifiableAttribute.DefaultValueType))
                    .Argument
                    .Value;
                var resolvedType = ModuleDefinition.ImportReference(property.PropertyType);
                var wrapperConstructor = resolvedType.Resolve().GetConstructors().FirstOrDefault(c =>
                    c.Parameters.Count == 1 &&
                    (expectedConstructorParamType == null ||
                        c.Parameters[0].ParameterType.FullName == expectedConstructorParamType.FullName));
                processorCommands.Add(p =>
                    p.Emit(OpCodes.Newobj, ModuleDefinition.ImportReference(wrapperConstructor)));
            }

            var getOrDefault = ReferenceFinder.GetMethodReference(
                    nullableReference,
                    methodDef => methodDef.Name == nameof(Nullable<int>.GetValueOrDefault) &&
                        methodDef.HasParameters)
                .MakeHostInstanceGeneric(property.PropertyType);
            processorCommands.Add(p => p.Emit(OpCodes.Call, getOrDefault));

            return processorCommands;
        }

        private void addProperty(
            TypeDefinition type, PropertyDefinition propertyBase, FieldReference fieldReference)
        {
            var propertyImpl = MethodHelpers.CreatePropertyImplementation(ModuleDefinition, propertyBase);
            var getMethodImpl = propertyImpl.GetMethod;

            var processor = getMethodImpl.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, fieldReference);
            processor.Emit(OpCodes.Ret);
            type.Properties.Add(propertyImpl);
            type.Methods.Add(getMethodImpl);
        }

        private void addCreateModifiableInstanceMethod(
            TypeDefinition type, GenericInstanceType genericParameterInterface, TypeReference modifiableType)
        {
            ImplementCreateModifiableInstanceMethod(type, genericParameterInterface, modifiableType, null);
        }
        
        private void addHasAttributeOfTypeMethod(
            TypeDefinition type, TypeReference genericParameterInterface, TypeReference modifiableType)
        {
            var methodBase =
                ReferenceFinder.GetMethodReference(genericParameterInterface, Constants.HasAttributeOfTypeMethod);
            var method = MethodHelpers.CreateMethodDefinitionFromInterfaceMethod(methodBase);

            var attributeIsKnownMethod = ReferenceFinder
                .GetMethodReference(modifiableType, Constants.AttributeIsKnownMethod);
            
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Call, attributeIsKnownMethod);
            processor.Emit(OpCodes.Ret);

            type.Methods.Add(method);
        }

        private void addModifyAttributeMethod(TypeDefinition type, TypeReference genericParameterInterface)
        {
            addInvalidOperationMethodOverride(
                type,
                genericParameterInterface,
                Constants.ModifyAttributeMethod,
                "Cannot modify attributes on immutable template.");
        }

        private void addInvalidOperationMethodOverride(
            TypeDefinition type, TypeReference genericParameterInterface, string methodName, string error)
        {
            var exceptionCtor = ReferenceFinder.GetConstructorReference(typeof(InvalidOperationException));

            var methodBase =
                ReferenceFinder.GetMethodReference(genericParameterInterface, methodName);
            var method = MethodHelpers.CreateMethodDefinitionFromInterfaceMethod(methodBase);

            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldstr, error);
            processor.Emit(OpCodes.Newobj, exceptionCtor);
            processor.Emit(OpCodes.Throw);

            type.Methods.Add(method);
        }
    }
}
