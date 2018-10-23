using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Mono.Cecil;
using Mono.Cecil.Cil;
using TypeSystem = Fody.TypeSystem;

namespace Weavers.TechEffects
{
    sealed class ImplementationWeaver
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly ILogger logger;
        private readonly ReferenceFinder referenceFinder;

        private readonly TemplateImplementationWeaver templateWeaver;

        private readonly MethodDefinition parametersTemplateDictionaryMethod;

        internal ImplementationWeaver(
            ModuleDefinition moduleDefinition,
            TypeSystem typeSystem,
            ILogger logger,
            ReferenceFinder referenceFinder)
        {
            this.moduleDefinition = moduleDefinition;
            this.logger = logger;
            this.referenceFinder = referenceFinder;

            templateWeaver = new TemplateImplementationWeaver(moduleDefinition, typeSystem, logger, referenceFinder);

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

            yield return createAndRegisterTemplateImplementation(interfaceToImplement, properties);
        }

        private TypeDefinition createAndRegisterTemplateImplementation(
            TypeReference interfaceToImplement, IReadOnlyCollection<PropertyDefinition> properties)
        {
            var type = templateWeaver.WeaveImplementation(interfaceToImplement, properties);
            registerInterfaceImplementation(interfaceToImplement, type);
            return type;
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
    }
}
