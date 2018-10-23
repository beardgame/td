using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TypeSystem = Fody.TypeSystem;

namespace Weavers.TechEffects
{
    abstract class BaseImplementationWeaver
    {
        protected ModuleDefinition ModuleDefinition { get; }
        protected TypeSystem TypeSystem { get; }
        protected ILogger Logger { get; }
        protected ReferenceFinder ReferenceFinder { get; }

        protected TypeReference ParametersTemplateInterface { get; }
        
        protected BaseImplementationWeaver(
            ModuleDefinition moduleDefinition,
            TypeSystem typeSystem,
            ILogger logger,
            ReferenceFinder referenceFinder)
        {
            ModuleDefinition = moduleDefinition;
            TypeSystem = typeSystem;
            Logger = logger;
            ReferenceFinder = referenceFinder;

            ParametersTemplateInterface = ReferenceFinder.GetTypeReference(Constants.Interface);
        }
        
        protected (TypeDefinition implementedType, GenericInstanceType genericParameterInterface) PrepareImplementation(
            TypeReference interfaceToImplement, string name, TypeReference baseClass)
        {
            var implementedType = new TypeDefinition(
                interfaceToImplement.Namespace,
                name,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                baseClass);

            var genericParameterInterface = ParametersTemplateInterface.MakeGenericInstanceType(interfaceToImplement);
            
            implementedType.AddInterfaceImplementation(interfaceToImplement);
            implementedType.AddInterfaceImplementation(genericParameterInterface);

            ModuleDefinition.Types.Add(implementedType);

            return (implementedType, genericParameterInterface);
        }

        protected void ImplementCreateModifiableInstanceMethod(
            TypeDefinition type,
            GenericInstanceType genericParameterInterface,
            TypeReference typeToCreate,
            FieldReference field)
        {
            var methodBase =
                ReferenceFinder
                    .GetMethodReference(genericParameterInterface, Constants.CreateModifiableInstanceMethod);
            methodBase.ReturnType = genericParameterInterface.GenericArguments[0];
            var method = MethodHelpers.CreateMethodDefinitionFromInterfaceMethod(methodBase);

            var ctor = ReferenceFinder.GetConstructorReference(typeToCreate);

            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            if (field != null)
            {
                processor.Emit(OpCodes.Ldfld, field);
            }
            processor.Emit(OpCodes.Newobj, ctor);
            processor.Emit(OpCodes.Ret);

            type.Methods.Add(method);
        }
    }
}