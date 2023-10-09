using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Microsoft.CodeAnalysis;

namespace Bearded.TD.Generators.TechEffects
{
    sealed class ParametersTemplateDefinition
    {
        public string Namespace { get; }
        private readonly string interfaceName;
        public ImmutableArray<ParametersPropertyDefinition> Properties { get; }

        public string TemplateName => baseName + "Template";
        public string ModifiableName => baseName + "Modifiable";
        private readonly string baseName;

        public string FullInterfaceName { get; }

        private ParametersTemplateDefinition(
            string @namespace,
            string interfaceName,
            ImmutableArray<INamedTypeSymbol> containingTypes,
            ImmutableArray<ParametersPropertyDefinition> properties)
        {
            Namespace = @namespace;
            this.interfaceName = interfaceName;
            Properties = properties;

            baseName = getInterfaceBaseName(interfaceName, containingTypes);
            FullInterfaceName = string.Join('.', containingTypes.Select(t => t.Name).Append(interfaceName));
        }

        public override string ToString()
        {
            var properties = string.Join(", ", Properties);
            return
                $"{nameof(Namespace)}: {Namespace}, " +
                $"{nameof(interfaceName)}: {interfaceName}, " +
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

            return new ParametersTemplateDefinition(
                $"{symbol.ContainingNamespace}", symbol.Name, getContainingTypes(symbol), properties);
        }

        private static ImmutableArray<INamedTypeSymbol> getContainingTypes(INamedTypeSymbol symbol)
        {
            var nestedTypeStack = new List<INamedTypeSymbol>();
            var containingType = symbol.ContainingType;
            while (containingType != null)
            {
                nestedTypeStack.Add(containingType);
                containingType = containingType.ContainingType;
            }

            return Enumerable.Reverse(nestedTypeStack).ToImmutableArray();
        }

        private static string getInterfaceBaseName(
            string interfaceName, ImmutableArray<INamedTypeSymbol> containingTypes)
        {
            if (interfaceName.EndsWith("Template")) interfaceName = interfaceName.Replace("Template", "");
            var rawBaseName = NameFactory.FromInterfaceName(interfaceName).ClassName();
            return string.Join("", containingTypes.Select(t => t.Name).Append(rawBaseName));
        }

        private static IEnumerable<ParametersPropertyDefinition> extractProperties(
            INamespaceOrTypeSymbol symbol,
            ISymbol modifiableAttributeSymbol,
            IDictionary<ITypeSymbol, IFieldSymbol> attributeConverters)
        {
            var properties = symbol.GetMembers()
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
                        var attributeTypeValue = (byte?) typeConstant?.Value ?? default(byte);
                        var attributeTypeEnum = (AttributeType) attributeTypeValue;
                        var attributeType = $"AttributeType.{attributeTypeEnum}";

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

            if(symbol is INamedTypeSymbol namedTypeSymbol)
            {
                properties = properties.Concat(namedTypeSymbol.Interfaces
                    .SelectMany(i => extractProperties(i, modifiableAttributeSymbol, attributeConverters)));
            }

            return properties;
        }

        public sealed class ParametersPropertyDefinition
        {
            public string Name { get; }
            public bool IsModifiable { get; }
            public string AttributeType { get; }
            public IParameterType Type { get; }
            public object? DefaultValue { get; }

            public ParametersPropertyDefinition(
                string name,
                string type,
                bool isModifiable,
                string attributeType,
                object? defaultValue,
                string? converter)
            {
                Name = name;
                Type = type switch
                {
                    _ when converter is { } => new ConverterParameterType(type, converter),
                    "double" => new SimpleParameterType("double"),
                    "bool" => new BooleanParameterType(),
                    _ => new CastParameterType(type)
                };

                IsModifiable = isModifiable;
                AttributeType = attributeType;
                DefaultValue = defaultValue;
            }

            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, " +
                    $"{nameof(IsModifiable)}: {IsModifiable} " +
                    $"{nameof(AttributeType)}: {AttributeType}" +
                    $"{nameof(Type)}: {Type}, " +
                    $"{nameof(DefaultValue)}: {DefaultValue}";
            }
        }

        public interface IParameterType
        {
            string TypeName { get; }

            string? Instantiation(object? value) => value == null ? null : $"{value}";
            string ToRaw(string input) => input;
            string FromRawConverter => "x => x";
        }

        private sealed class SimpleParameterType : IParameterType
        {
            public string TypeName { get; }

            public SimpleParameterType(string type)
            {
                TypeName = type;
            }
        }

        private sealed class CastParameterType : IParameterType
        {
            public string TypeName { get; }

            public string FromRawConverter => $"x => ({TypeName}) x";

            public CastParameterType(string type)
            {
                TypeName = type;
            }
        }

        private sealed class ConverterParameterType : IParameterType
        {
            public string TypeName { get; }

            private readonly string converter;

            public string? Instantiation(object? value) => value == null ? null : $"{converter}.ToWrapped({value})";

            public string ToRaw(string input) => $"{converter}.ToRaw({input})";

            public string FromRawConverter => $"{converter}.ToWrapped";

            public ConverterParameterType(string type, string converter)
            {
                TypeName = type;
                this.converter = converter;
            }
        }

        private sealed class BooleanParameterType : IParameterType
        {
            public string TypeName => "bool";

            public string? Instantiation(object? value) => value == null ? null : $"{value}".ToLower();

            public string ToRaw(string input) => $"Convert.ToDouble({input})";

            public string FromRawConverter => "x => Convert.ToBoolean(x)";
        }
    }
}
