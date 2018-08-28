using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

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

        public static string ToCamelCase(this string titleCaseName)
        {
            return char.ToLowerInvariant(titleCaseName[0]) + titleCaseName.Substring(1);
        }
    }
}
