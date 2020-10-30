using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators.TechEffects
{
    sealed class ParametersTemplateDefinition
    {
        public string Namespace { get; }
        public string InterfaceName { get; }
        public ImmutableArray<ParametersPropertyDefinition> Properties { get; }

        public string TemplateName => baseName + "Template";
        public string ModifiableName => baseName + "Modifiable";
        private string baseName => getInterfaceBaseName(InterfaceName);

        private ParametersTemplateDefinition(
            string @namespace, string interfaceName, ImmutableArray<ParametersPropertyDefinition> properties)
        {
            Namespace = @namespace;
            InterfaceName = interfaceName;
            Properties = properties;
        }

        public override string ToString()
        {
            var properties = string.Join(", ", Properties);
            return
                $"{nameof(Namespace)}: {Namespace}, " +
                $"{nameof(InterfaceName)}: {InterfaceName}, " +
                $"{nameof(Properties)}: {properties}, " +
                $"{nameof(TemplateName)}: {TemplateName}, " +
                $"{nameof(ModifiableName)}: {ModifiableName}";
        }

        public static ParametersTemplateDefinition FromNamedSymbol(
            INamedTypeSymbol symbol,
            INamedTypeSymbol modifiableAttributeSymbol,
            IDictionary<ITypeSymbol, IFieldSymbol> attributeConverters)
        {
            var properties =
                extractProperties(symbol, modifiableAttributeSymbol, attributeConverters).ToImmutableArray();

            return new ParametersTemplateDefinition($"{symbol.ContainingNamespace}", symbol.Name, properties);
        }

        private static string getInterfaceBaseName(string interfaceName)
        {
            if (interfaceName[0] == 'I') interfaceName = interfaceName.Substring(1);
            if (interfaceName.EndsWith("Template")) interfaceName = interfaceName.Replace("Template", "");
            return interfaceName;
        }

        private static IEnumerable<ParametersPropertyDefinition> extractProperties(
            INamespaceOrTypeSymbol symbol,
            ISymbol modifiableAttributeSymbol,
            IDictionary<ITypeSymbol, IFieldSymbol> attributeConverters)
        {
            return symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Select(propertySymbol =>
                    {
                        var modifiableAttribute = propertySymbol.GetAttributes()
                            .FirstOrDefault(a =>
                                a.AttributeClass!.Equals(modifiableAttributeSymbol, SymbolEqualityComparer.Default));
                        var defaultConstant = modifiableAttribute?.ConstructorArguments.FirstOrDefault();
                        var defaultValue = defaultConstant?.Value;
                        var typeConstant = modifiableAttribute?.NamedArguments
                            .FirstOrDefault(pair => pair.Key == "Type").Value;
                        var attributeType = typeConstant?.Value == null
                            ? "AttributeType.None"
                            : $"(AttributeType) {typeConstant.Value.Value}";

                        attributeConverters.TryGetValue(propertySymbol.Type, out var converter);
                        return new ParametersPropertyDefinition(
                            propertySymbol.Name,
                            $"{propertySymbol.Type}",
                            modifiableAttribute != null,
                            attributeType,
                            defaultValue,
                            converter == null ? null : $"{converter}");
                    }
                );
        }

        public sealed class ParametersPropertyDefinition
        {
            public string Name { get; }
            public string Type { get; }
            public bool IsModifiable { get; }
            public string AttributeType { get; }
            public object? DefaultValue { get; }
            public string? Converter { get; }

            public string? DefaultValueInstantiation => DefaultValue == null
                ? null
                : Converter == null ? $"{DefaultValue}" : $"{Converter}.ToWrapped({DefaultValue})";

            public ParametersPropertyDefinition(
                string name,
                string type,
                bool isModifiable,
                string attributeType,
                object? defaultValue,
                string? converter)
            {
                Name = name;
                Type = type;
                IsModifiable = isModifiable;
                AttributeType = attributeType;
                DefaultValue = defaultValue;
                Converter = converter;
            }

            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, " +
                    $"{nameof(Type)}: {Type}, " +
                    $"{nameof(IsModifiable)}: {IsModifiable} " +
                    $"{nameof(AttributeType)}: {AttributeType}" +
                    $"{nameof(DefaultValue)}: {DefaultValue}" +
                    $"{nameof(Converter)}: {Converter}" +
                    $"{nameof(DefaultValueInstantiation)}: {DefaultValueInstantiation}";
            }
        }
    }
}
