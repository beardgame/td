using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Mono.Cecil;

namespace Weavers.TechEffects
{
    sealed class AttributeConverters
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly ReferenceFinder referenceFinder;
        private readonly IReadOnlyDictionary<string, FieldReference> converters;

        private AttributeConverters(
            ModuleDefinition moduleDefinition,
            ReferenceFinder referenceFinder,
            IReadOnlyDictionary<string, FieldReference> converters)
        {
            this.moduleDefinition = moduleDefinition;
            this.referenceFinder = referenceFinder;
            this.converters = converters;
        }

        public FieldReference FieldForConversion(TypeReference type)
        {
            if (converters.TryGetValue(type.FullName, out var field)) return field;

            throw new Exception($"Could not find an attribute converter for type {type.FullName}");
        }

        public MethodReference MethodForConversionToRaw(FieldReference field) =>
            referenceFinder.GetMethodReference(field.FieldType, Constants.ToRawMethod);

        public MethodReference MethodForConversionToWrapped(FieldReference field) =>
            referenceFinder.GetMethodReference(field.FieldType, Constants.ToWrappedMethod);

        internal static AttributeConverters Create(
            ModuleDefinition moduleDefinition,
            ReferenceFinder referenceFinder)
        {
            var containsAttributeConvertersType =
                referenceFinder.GetTypeReference<ContainsAttributeConvertersAttribute>();
            var convertsAttributeType =
                referenceFinder.GetTypeReference<ConvertsAttributeAttribute>();

            var typesWithConverters = moduleDefinition.Types.Where(
                type => type.CustomAttributes.Any(
                    customAttribute => isCustomAttributeOfType(customAttribute, containsAttributeConvertersType)));
            var converters = typesWithConverters.SelectMany(type => type.Fields).Where(
                    field => field.CustomAttributes.Any(
                        customAttribute => isCustomAttributeOfType(customAttribute, convertsAttributeType)))
                .Select(moduleDefinition.ImportReference)
                .Select(f => f.Resolve());

            var dict = new Dictionary<string, FieldReference>();
            var attributeConverterType = referenceFinder.GetTypeReference(typeof(AttributeConverter<>));

            foreach (var converter in converters)
            {
                if (!converter.IsStatic)
                {
                    throw new Exception("Attribute converter fields always have to be static.");
                }

                var fieldType = converter.FieldType;
                if (!fieldType.FullName.StartsWith(attributeConverterType.FullName))
                {
                    throw new Exception($"Attribute converter fields always should be of type {attributeConverterType.Name}.");
                }
                if (!(fieldType is GenericInstanceType genericFieldType))
                {
                    throw new Exception("Attribute converter expected to be a generic instance.");
                }
                if (genericFieldType.GenericArguments.Count != 1)
                {
                    throw new Exception("Incorrect number of generic arguments for attribute converter type.");
                }

                dict.Add(genericFieldType.GenericArguments[0].FullName, converter);
            }

            return new AttributeConverters(moduleDefinition, referenceFinder, dict);
        }

        private static bool isCustomAttributeOfType(CustomAttribute customAttribute, MemberReference desiredType)
            => customAttribute.Constructor?.DeclaringType?.FullName == desiredType.FullName;
    }
}
