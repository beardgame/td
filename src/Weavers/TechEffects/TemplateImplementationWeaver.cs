using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Newtonsoft.Json;
using TypeSystem = Fody.TypeSystem;

namespace Weavers.TechEffects
{
    sealed class TemplateImplementationWeaver
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly TypeSystem typeSystem;
        private readonly ILogger logger;
        private readonly ReferenceFinder referenceFinder;

        private readonly TypeReference parametersTemplateInterface;

        public TemplateImplementationWeaver(
            ModuleDefinition moduleDefinition,
            TypeSystem typeSystem,
            ILogger logger,
            ReferenceFinder referenceFinder)
        {
            this.moduleDefinition = moduleDefinition;
            this.typeSystem = typeSystem;
            this.logger = logger;
            this.referenceFinder = referenceFinder;
            
            parametersTemplateInterface = this.referenceFinder.GetTypeReference(Constants.Interface);
        }

        public TypeDefinition WeaveImplementation(
            TypeReference interfaceToImplement,
            IReadOnlyCollection<PropertyDefinition> properties)
        {
            var templateType = new TypeDefinition(
                interfaceToImplement.Namespace,
                Constants.GetTemplateClassNameForInterface(interfaceToImplement.Name),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                typeSystem.ObjectReference);

            var genericParameterInterface = parametersTemplateInterface.MakeGenericInstanceType(interfaceToImplement);
            
            templateType.AddInterfaceImplementation(interfaceToImplement);
            templateType.AddInterfaceImplementation(genericParameterInterface);
            
            var fieldsByProperty = addTemplateConstructor(templateType, properties);

            foreach (var entry in fieldsByProperty)
            {
                addTemplateProperty(templateType, entry.Key, entry.Value);
            }

            addTemplateModifiableCreationMethod(templateType, genericParameterInterface);
            addTemplateModifyMethod(templateType, genericParameterInterface);

            return templateType;
        }

        private Dictionary<PropertyDefinition, FieldReference> addTemplateConstructor(
            TypeDefinition type, IReadOnlyCollection<PropertyDefinition> properties)
        {
            var nullable = referenceFinder.GetTypeReference(typeof(Nullable<>));

            var method = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName
                | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                typeSystem.VoidReference);

            method.AddCustomAttribute(typeof(JsonConstructorAttribute), referenceFinder);

            var propertyFields = new List<(FieldDefinition, List<Action<ILProcessor>>)>();
            var fieldsByProperty = new Dictionary<PropertyDefinition, FieldReference>();

            foreach (var property in properties)
            {
                property.TryGetCustomAttribute(typeof(ModifiableAttribute), out var modifiableAttribute);
                var attributeParameters = modifiableAttribute.Constructor.Resolve().Parameters;

                var parameterType = attributeParameters.Count == 0
                    ? property.PropertyType
                    : nullable.MakeGenericInstanceType(property.PropertyType);
                method.Parameters.Add(
                    new ParameterDefinition(
                        property.Name.ToCamelCase(), ParameterAttributes.None, parameterType));

                var fieldDef = new FieldDefinition(
                    property.Name.ToCamelCase(),
                    FieldAttributes.Private | FieldAttributes.InitOnly,
                    property.PropertyType);

                type.Fields.Add(fieldDef);

                var processorCommands = new List<Action<ILProcessor>>();
                if (attributeParameters.Count > 0)
                {
                    var defaultValue = (CustomAttributeArgument) modifiableAttribute.ConstructorArguments[0].Value;

                    processorCommands.Add(p => ILHelpers.EmitLd(p, defaultValue.Type, defaultValue.Value));

                    if (!property.PropertyType.IsPrimitive)
                    {
                        var expectedConstructorParamType = (Type) modifiableAttribute.Properties
                            .FirstOrDefault(p => p.Name == nameof(ModifiableAttribute.DefaultValueType))
                            .Argument
                            .Value;
                        var resolvedType = moduleDefinition.ImportReference(property.PropertyType);
                        var wrapperConstructor = resolvedType.Resolve().GetConstructors().FirstOrDefault(c =>
                            c.Parameters.Count == 1 &&
                            (expectedConstructorParamType == null ||
                                c.Parameters[0].ParameterType.FullName == expectedConstructorParamType.FullName));
                        processorCommands.Add(p =>
                            p.Emit(OpCodes.Newobj, moduleDefinition.ImportReference(wrapperConstructor)));
                    }

                    var getOrDefault = referenceFinder.GetMethodReference(
                            nullable,
                            methodDef => methodDef.Name == nameof(Nullable<int>.GetValueOrDefault) &&
                                methodDef.HasParameters)
                        .MakeHostInstanceGeneric(property.PropertyType);
                    processorCommands.Add(p => p.Emit(OpCodes.Call, getOrDefault));
                }

                propertyFields.Add((fieldDef, processorCommands));
                fieldsByProperty.Add(property, fieldDef);
            }

            var objectConstructor = referenceFinder.GetConstructorReference(typeSystem.ObjectDefinition);
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

        private void addTemplateProperty(
            TypeDefinition type, PropertyDefinition propertyBase, FieldReference fieldReference)
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

        private void addTemplateModifiableCreationMethod(TypeDefinition type, GenericInstanceType genericParameterInterface)
        {
            var methodBase =
                referenceFinder
                    .GetMethodReference(genericParameterInterface, Constants.CreateModifiableInstanceMethod);
            methodBase.ReturnType = genericParameterInterface.GenericArguments[0];
            var method = MethodHelpers.CreateMethodDefinitionFromInterfaceMethod(methodBase);

            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldnull);
            processor.Emit(OpCodes.Ret);

            type.Methods.Add(method);
        }

        private void addTemplateModifyMethod(TypeDefinition type, TypeReference genericParameterInterface)
        {
            var exceptionCtor = referenceFinder.GetConstructorReference(typeof(InvalidOperationException));

            var methodBase =
                referenceFinder.GetMethodReference(genericParameterInterface, Constants.ModifyAttributeMethod);
            var method = MethodHelpers.CreateMethodDefinitionFromInterfaceMethod(methodBase);

            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldstr, "Cannot modify attributes on immutable template.");
            processor.Emit(OpCodes.Newobj, exceptionCtor);
            processor.Emit(OpCodes.Throw);

            type.Methods.Add(method);
        }
    }
}