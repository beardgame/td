using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Weavers
{
    static class MethodHelpers
    {
        public static MethodDefinition CreateMethodDefinitionFromInterfaceMethod(MethodReference methodBase)
        {
            var method = new MethodDefinition(
                methodBase.Name,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                methodBase.ReturnType);

            foreach (var methodBaseParameter in methodBase.Parameters)
            {
                method.Parameters.Add(
                    new ParameterDefinition(
                        methodBaseParameter.Name, methodBaseParameter.Attributes, methodBaseParameter.ParameterType));
            }

            method.Body = new MethodBody(method);
            return method;
        }
    }
}
