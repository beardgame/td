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

        protected void AddVirtualMethodImplementation(
            TypeReference interfaceToImplement, TypeDefinition type, MethodReference baseMethod)
        {
            var method = new MethodDefinition(
                baseMethod.Name,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig
                | MethodAttributes.ReuseSlot | MethodAttributes.Virtual,
                baseMethod.ReturnType);
            foreach (var p in baseMethod.Parameters)
            {
                method.Parameters.Add(new ParameterDefinition(
                    p.Name, p.Attributes, ModuleDefinition.ImportReference(p.ParameterType)));
            }

            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            for (var i = 1; i <= baseMethod.Parameters.Count; i++)
            {
                processor.Emit(OpCodes.Ldarg, i);
            }

            processor.Emit(OpCodes.Call, baseMethod);
            processor.Emit(OpCodes.Ret);

            var interfaceMethod = ReferenceFinder.GetMethodReference(interfaceToImplement, baseMethod.Name);
            method.Overrides.Add(ModuleDefinition.ImportReference(interfaceMethod));
            type.Methods.Add(method);
        }
    }
}