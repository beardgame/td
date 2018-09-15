using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Weavers
{
    static class Extensions
    {
        public static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T> elements)
        {
            foreach (var element in elements)
            {
                queue.Enqueue(element);
            }
        }

        public static void AddInterfaceImplementation(this TypeDefinition type, TypeReference interfaceType)
        {
            var impl = new InterfaceImplementation(interfaceType);
            type.Interfaces.Add(impl);
        }

        public static void AddCustomAttribute(this MethodDefinition method, Type attributeType,
            ReferenceFinder referenceFinder)
        {
            var jsonAttributeConstructor =
                referenceFinder.GetMethodReference(attributeType, m => m.IsConstructor);
            method.CustomAttributes.Add(new CustomAttribute(jsonAttributeConstructor));
        }

        public static bool ImplementsInterface(this TypeDefinition type, Type interfaceType)
        {
            return type.HasInterfaces
                && type.Interfaces.Any(interfaceImplementation =>
                    interfaceImplementation.InterfaceType.FullName == interfaceType.FullName);
        }

        public static bool TryGetCustomAttribute(this PropertyDefinition property, Type attributeType, out CustomAttribute attribute)
        {
            if (!property.HasCustomAttributes)
            {
                attribute = default(CustomAttribute);
                return false;
            }

            attribute = property.CustomAttributes.FirstOrDefault(a => a.Constructor?.DeclaringType?.FullName == attributeType.FullName);

            return attribute != default(CustomAttribute);
        }

        public static MethodReference MakeHostInstanceGeneric(
            this MethodReference self,
            params TypeReference[] args)
        {
            var reference = new MethodReference(
                self.Name,
                self.ReturnType,
                self.DeclaringType.MakeGenericInstanceType(args))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in self.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
            }

            return reference;
        }

        public static string ToCamelCase(this string titleCaseName)
        {
            return char.ToLowerInvariant(titleCaseName[0]) + titleCaseName.Substring(1);
        }
    }
}
