using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Weavers
{
    static class Extensions
    {
        public static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T> elements)
        {
            foreach (var elmt in elements) queue.Enqueue(elmt);
        }

        public static bool ImplementsInterface(this TypeDefinition type, string interfaceName)
        {
            return type.HasInterfaces
                && type.Interfaces.Any(interfaceImplementation =>
                    interfaceImplementation.InterfaceType.Name == interfaceName);
        }

        public static bool TryGetCustomAttribute(this PropertyDefinition property, string attributeName, out CustomAttribute attribute)
        {
            if (!property.HasCustomAttributes)
            {
                attribute = default(CustomAttribute);
                return false;
            }

            attribute = property.CustomAttributes.FirstOrDefault(a => a.Constructor?.DeclaringType?.Name == attributeName);

            return attribute != default(CustomAttribute);
        }

        public static string ToCamelCase(this string titleCaseName)
        {
            return char.ToLowerInvariant(titleCaseName[0]) + titleCaseName.Substring(1);
        }
    }
}
