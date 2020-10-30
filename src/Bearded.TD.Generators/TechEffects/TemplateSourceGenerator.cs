using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Bearded.TD.Generators.TechEffects.TechEffectsUtils;

namespace Bearded.TD.Generators.TechEffects
{
    sealed class TemplateSourceGenerator
    {
        public static string GenerateFor(ParametersTemplateDefinition definition)
        {
            return new TemplateSourceGenerator()
                .addFileTop(definition.Namespace, definition.TemplateName, definition.InterfaceName)
                .addProperties(
                    definition.Properties.Select(property => (Type: property.Type, PropertyName: property.Name)))
                .addConstructor(
                    definition.TemplateName,
                    definition.Properties.Select(property => (
                        Type: property.Type,
                        PropertyName: property.Name,
                        DefaultValue: (string?) null)))
                .addHasAttributeOfTypeMethod(definition.ModifiableName)
                .addCreateModifiableInstanceMethod(definition.InterfaceName, definition.ModifiableName)
                .addFileBottom()
                .build();
        }

        private readonly StringBuilder sb = new StringBuilder();

        private TemplateSourceGenerator addFileTop(string @namespace, string className, string interfaceName)
        {
            sb.Append(Templates.FileHeader);
            sb.Append(Templates.ClassTop(@namespace, className, interfaceName));
            return this;
        }

        private TemplateSourceGenerator addProperties(IEnumerable<(string Type, string PropertyName)> properties)
        {
            foreach (var (type, name) in properties)
            {
                sb.Append(Templates.Property(type, name));
            }
            return this;
        }

        private TemplateSourceGenerator addConstructor(
            string className, IEnumerable<(string Type, string PropertyName, string? DefaultValue)> parameters)
        {
            sb.Append(Templates.Constructor(className, parameters));
            return this;
        }

        private TemplateSourceGenerator addHasAttributeOfTypeMethod(string modifiableName)
        {
            sb.Append(Templates.HasAttributeOfTypeMethod(modifiableName));
            return this;
        }

        private TemplateSourceGenerator addCreateModifiableInstanceMethod(
            string interfaceName, string modifiableClassName)
        {
            sb.Append(Templates.CreateModifiableMethod(interfaceName, modifiableClassName));
            return this;
        }

        private TemplateSourceGenerator addFileBottom()
        {
            sb.Append(Templates.ClassBottom);
            return this;
        }

        private string build() => sb.ToString();

        private static class Templates
        {
            public const string FileHeader = @"
using System;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Newtonsoft.Json;
";

            public static string ClassTop(string @namespace, string className, string interfaceName) => $@"
namespace {@namespace}
{{
    [ParametersTemplate(typeof({interfaceName}))]
    sealed class {className} : TemplateBase, {interfaceName}
    {{
";

            public const string ClassBottom = @"
    }
}
";

            public static string Property(string type, string name) => $@"
        public {type} {name} {{ get; }}
";

            public static string Constructor(
                string className, IEnumerable<(string Type, string PropertyName, string? DefaultValue)> parameters)
            {
                var parametersList = parameters.ToList();
                var parametersString = string.Join(", ",
                    parametersList.Select(tuple =>
                        constructorParameter(tuple.Type, tuple.PropertyName, tuple.DefaultValue != null)));
                var bodyString = string.Join("\n",
                    parametersList.Select(tuple =>
                        constructorAssignment(tuple.PropertyName, tuple.DefaultValue)));

                return $@"
        [JsonConstructor]
        public {className}({parametersString})
        {{
{bodyString}
        }}
";
            }

            private static string constructorParameter(string type, string propertyName, bool isNullable)
            {
                var parameterType = isNullable ? $"{type}?" : type;
                return $"{parameterType} {toCamelCase(propertyName)}";
            }

            private static string constructorAssignment(string propertyName, string? defaultValue)
            {
                var val = toCamelCase(propertyName);
                if (defaultValue != null)
                {
                    val += $".GetValueOrDefault({defaultValue})";
                }

                return $@"
            {propertyName} = {val};";
            }

            public static string HasAttributeOfTypeMethod(string modifiableClassName) => $@"
        public bool HasAttributeOfType(AttributeType type) => {modifiableClassName}.AttributeIsKnown(type);";

            public static string CreateModifiableMethod(string interfaceName, string modifiableClassName) => $@"
        public {interfaceName} CreateModifiableInstance() => new {modifiableClassName}(this);
";
        }
    }
}
