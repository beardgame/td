using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators.TechEffects
{
    sealed class ParametersTemplateDefinition
    {
        public string BaseName { get; }
        public ImmutableArray<ParametersPropertyDefinition> Properties { get; }

        public string TemplateName => BaseName + "Template";
        public string ModifiableName => BaseName + "Modifiable";

        private ParametersTemplateDefinition(string baseName, ImmutableArray<ParametersPropertyDefinition> properties)
        {
            BaseName = baseName;
            Properties = properties;
        }

        public override string ToString()
        {
            var properties = string.Join(", ", Properties);
            return
                $"{nameof(BaseName)}: {BaseName}, " +
                $"{nameof(Properties)}: {properties}, " +
                $"{nameof(TemplateName)}: {TemplateName}, " +
                $"{nameof(ModifiableName)}: {ModifiableName}";
        }

        public static ParametersTemplateDefinition FromNamedSymbol(
            INamedTypeSymbol symbol, INamedTypeSymbol modifiableAttributeSymbol)
        {
            var baseName = getInterfaceBaseName(symbol.Name);
            var properties = extractProperties(symbol, modifiableAttributeSymbol).ToImmutableArray();

            return new ParametersTemplateDefinition(baseName, properties);
        }

        private static string getInterfaceBaseName(string interfaceName)
        {
            if (interfaceName[0] == 'I') interfaceName = interfaceName.Substring(1);
            if (interfaceName.EndsWith("Template")) interfaceName = interfaceName.Replace("Template", "");
            return interfaceName;
        }

        private static IEnumerable<ParametersPropertyDefinition> extractProperties(
            INamespaceOrTypeSymbol symbol, ISymbol modifiableAttributeSymbol)
        {
            return symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Select(propertySymbol =>
                    {
                        var modifiableAttribute = propertySymbol.GetAttributes()
                            .FirstOrDefault(a =>
                                a.AttributeClass!.Equals(modifiableAttributeSymbol, SymbolEqualityComparer.Default));
                        var typeConstant = modifiableAttribute?.NamedArguments.FirstOrDefault(pair => pair.Key == "Type").Value;

                        return new ParametersPropertyDefinition(
                            propertySymbol.Name, propertySymbol.Type, modifiableAttribute != null, typeConstant);
                    }
                );
        }

        public sealed class ParametersPropertyDefinition
        {
            public string Name { get; }
            public ITypeSymbol Type { get; }
            public bool IsModifiable { get; }
            public TypedConstant? TypeConstant { get; }

            public ParametersPropertyDefinition(
                string name, ITypeSymbol type, bool isModifiable, TypedConstant? typeConstant)
            {
                Name = name;
                Type = type;
                IsModifiable = isModifiable;
                // TODO: access to the actual enum constant can be done by finding the enum symbol and resolving number
                TypeConstant = typeConstant;
            }

            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, " +
                    $"{nameof(Type)}: {Type}, " +
                    $"{nameof(IsModifiable)}: {IsModifiable} " +
                    $"{nameof(TypeConstant)}: {TypeConstant?.Value}";
            }
        }
    }
}
