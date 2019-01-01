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

        private readonly ModifiableImplementationWeaver modifiableWeaver;
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

            modifiableWeaver = new ModifiableImplementationWeaver(moduleDefinition, typeSystem, logger, referenceFinder);
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
                createImplementations(@interface);
            }

            postProcessTypeDictionary();
        }

        private void preProcessTypeDictionary()
        {
            var dictConstructor = referenceFinder
                .GetConstructorReference(referenceFinder.GetTypeReference(typeof(Dictionary<Type, Type>)));

            parametersTemplateDictionaryMethod.Body =
                new MethodBody(parametersTemplateDictionaryMethod) {InitLocals = true};
            parametersTemplateDictionaryMethod.Body.Variables.Add(new VariableDefinition(dictConstructor.DeclaringType));

            var processor = parametersTemplateDictionaryMethod.Body.GetILProcessor();
            processor.Emit(OpCodes.Newobj, dictConstructor);
            processor.Emit(OpCodes.Stloc_0);
        }

        private void postProcessTypeDictionary()
        {
            var readOnlyDictConstructor = referenceFinder
                .GetConstructorReference(typeof(ReadOnlyDictionary<Type, Type>));

            var processor = parametersTemplateDictionaryMethod.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Newobj, readOnlyDictConstructor);
            processor.Emit(OpCodes.Ret);
        }

        private void createImplementations(TypeDefinition interfaceToImplement)
        {
            logger.LogInfo($"Weaving implementations for {interfaceToImplement}.");

            var properties = interfaceToImplement.Properties
                .Where(p => p.TryGetCustomAttribute(Constants.ModifiableAttribute, out _))
                .ToList();

            var modifiableType = modifiableWeaver.WeaveImplementation(interfaceToImplement, properties);
            var templateType = templateWeaver.WeaveImplementation(interfaceToImplement, properties, modifiableType);
            registerInterfaceImplementation(interfaceToImplement, templateType);
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
