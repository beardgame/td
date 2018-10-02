using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
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

        private readonly TypeReference parametersTemplateInterface;
        private readonly MethodDefinition parametersTemplateDictionaryMethod;

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

            parametersTemplateInterface = this.referenceFinder.GetTypeReference(Constants.Interface);

            var techEffectLibraryBase = this.moduleDefinition.ImportReference(typeof(ParametersTemplateLibrary<>));
            var techEffectLibrary =
                this.moduleDefinition.Types.FirstOrDefault(type =>
                    type?.BaseType != null
                        && type.BaseType.FullName.StartsWith(techEffectLibraryBase.FullName));
            parametersTemplateDictionaryMethod = referenceFinder
                .GetMethodReference(techEffectLibrary, method => method.Name == "GetInterfaceToTemplateMap").Resolve();
        }

        public void Execute()
        {
            preProcessTypeDictionary();

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

            postPrecessTypeDictionary();
        }

        private void preProcessTypeDictionary()
        {
            var typeTypeReference = referenceFinder.GetTypeReference<Type>();
            var dictConstructor = referenceFinder
                .GetConstructorReference(typeof(Dictionary<Type, Type>))
                .MakeHostInstanceGeneric(typeTypeReference, typeTypeReference);

            parametersTemplateDictionaryMethod.Body =
                new MethodBody(parametersTemplateDictionaryMethod) {InitLocals = true};
            parametersTemplateDictionaryMethod.Body.Variables.Add(new VariableDefinition(dictConstructor.DeclaringType));

            var processor = parametersTemplateDictionaryMethod.Body.GetILProcessor();
            processor.Emit(OpCodes.Newobj, dictConstructor);
            processor.Emit(OpCodes.Stloc_0);
        }

        private void postPrecessTypeDictionary()
        {
            var typeTypeReference = referenceFinder.GetTypeReference<Type>();
            var readOnlyDictConstructor = referenceFinder
                .GetConstructorReference(typeof(ReadOnlyDictionary<Type, Type>))
                .MakeHostInstanceGeneric(typeTypeReference, typeTypeReference);

            var processor = parametersTemplateDictionaryMethod.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Newobj, readOnlyDictConstructor);
            processor.Emit(OpCodes.Ret);
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

            var genericTechEffectInterface = parametersTemplateInterface.MakeGenericInstanceType(interfaceToImplement);
            
            templateType.AddInterfaceImplementation(interfaceToImplement);
            templateType.AddInterfaceImplementation(genericTechEffectInterface);
            
            var fieldsByProperty = addTemplateConstructor(templateType, properties);

            foreach (var entry in fieldsByProperty)
            {
                addTemplateProperty(templateType, entry.Key, entry.Value);
            }

            registerInterfaceImplementation(interfaceToImplement, templateType);

            return templateType;
        }

        private void registerInterfaceImplementation(TypeReference interfaceToImplement, TypeReference templateType)
        {
            var @typeOf = referenceFinder.GetMethodReference<Type>(
                type => Type.GetTypeFromHandle(default(RuntimeTypeHandle)));

            var processor = parametersTemplateDictionaryMethod.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldloc_0);

            processor.Emit(OpCodes.Ldtoken, interfaceToImplement);
            processor.Emit(OpCodes.Call, @typeOf);
            processor.Emit(OpCodes.Ldtoken, templateType);
            processor.Emit(OpCodes.Call, @typeOf);

            var addMethodReference =
                referenceFinder.GetMethodReference<Dictionary<Type, Type>>(dict => dict.Add(null, null));
            processor.Emit(OpCodes.Callvirt, addMethodReference);
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
    }
}
