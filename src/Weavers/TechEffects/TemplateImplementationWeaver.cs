using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
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
                ModuleDefinition moduleDefinition,
                TypeSystem typeSystem,
                ILogger logger,
                ReferenceFinder referenceFinder,
                AttributeConverters attributeConverters)
            : base(moduleDefinition, typeSystem, logger, referenceFinder, attributeConverters)
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
            addModificationMethods(templateType, genericParameterInterface);

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

                Maybe<VariableDefinition> localVar = Maybe.Nothing;
                var processorCommands = attributeParameters.Count > 0
                    ? getConstructorCommandsForProperty(property, modifiableAttribute, out localVar)
                    : new List<Action<ILProcessor>>();

                localVar.Match(varDef =>
                {
                    method.Body.Variables.Add(varDef);
                    method.Body.InitLocals = true;
                });

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
            PropertyReference property, ICustomAttribute modifiableAttribute, out Maybe<VariableDefinition> localVarOut)
        {
            localVarOut = Maybe.Nothing;

            var processorCommands = new List<Action<ILProcessor>>();
            var defaultValue = (CustomAttributeArgument) modifiableAttribute.ConstructorArguments[0].Value;

            if (property.PropertyType.IsPrimitive)
            {
                processorCommands.Add(p => ILHelpers.EmitLd(p, defaultValue.Type, defaultValue.Value));
            }
            else
            {
                processorCommands.Add(p => ILHelpers.EmitLd(p, TypeSystem.DoubleReference, defaultValue.Value));

                // push to local variable since we need the attribute converter below it on the stack
                var localVar = new VariableDefinition(TypeSystem.DoubleReference);
                localVarOut = Maybe.Just(localVar);
                processorCommands.Add(p => p.Emit(OpCodes.Stloc, localVar));

                // load the static field
                var converterField = AttributeConverters.FieldForConversion(property.PropertyType);
                processorCommands.Add(p => p.Emit(OpCodes.Ldsfld, converterField));
                // push local variable on stack
                processorCommands.Add(p => p.Emit(OpCodes.Ldloc, localVar));
                // call converter method
                processorCommands.Add(p => p.Emit(
                    OpCodes.Callvirt, AttributeConverters.MethodForConversionToWrapped(converterField)));
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

        private void addModificationMethods(TypeDefinition type, TypeReference genericParameterInterface)
        {
            foreach (var methodName in Constants.ModificationMethods)
            {
                addInvalidOperationMethodOverride(
                    type,
                    genericParameterInterface,
                    methodName,
                    "Cannot modify attributes on immutable template.");
            }
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
