using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Bearded.TD.Generators.TechEffects.ParametersTemplateDefinition;
using static Bearded.TD.Generators.TechEffects.TechEffectsUtils;

namespace Bearded.TD.Generators.TechEffects
{
    sealed class TemplateSourceGenerator
    {
        public static string GenerateFor(ParametersTemplateDefinition definition)
        {
            return new TemplateSourceGenerator()
                .addFileTop(definition.Namespace, definition.TemplateName, definition.FullInterfaceName)
                .addProperties(definition.Properties)
                .addConstructor(definition.TemplateName, definition.Properties)
                .addHasAttributeOfTypeMethod(definition.ModifiableName)
                .addCreateModifiableInstanceMethod(definition.FullInterfaceName, definition.ModifiableName)
                .addFileBottom()
                .build();
        }

        private readonly StringBuilder sb = new();

        private TemplateSourceGenerator addFileTop(string @namespace, string className, string interfaceName)
        {
            sb.Append(Templates.FileHeader);
            sb.Append(Templates.ClassTop(@namespace, className, interfaceName));
            return this;
        }

        private TemplateSourceGenerator addProperties(IEnumerable<ParametersPropertyDefinition> properties)
        {
            foreach (var p in properties)
            {
                sb.Append(Templates.Property(p.Type.TypeName, p.Name));
            }
            sb.Append(Strings.NewLine);
            return this;
        }

        private TemplateSourceGenerator addConstructor(
            string className, IEnumerable<ParametersPropertyDefinition> parameters)
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
            public const string FileHeader = @"// This file is generated by Bearded.TD.Generators

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
    {{";

            public const string ClassBottom = @"
    }
}
";

            public static string Property(string type, string name) => $@"
        public {type} {name} {{ get; }}";

            public static string Constructor(
                string className, IEnumerable<ParametersPropertyDefinition> parameters)
            {
                var parametersList = parameters.ToList();
                var parametersString = string.Join(",",
                    parametersList.Select(p => constructorParameter(p.Type.TypeName, p.Name, p.DefaultValue != null)));
                var bodyString = string.Join("",
                    parametersList.Select(p => constructorAssignment(p.Name, p.Type, p.DefaultValue)));

                return $@"
        [JsonConstructor]
        public {className}({parametersString})
        {{{bodyString}
        }}
";
            }

            private static string constructorParameter(string type, string name, bool isNullable)
            {
                var parameterType = isNullable ? $"{type}?" : type;
                return $@"
            {parameterType} {ToCamelCase(name)}";
            }

            private static string constructorAssignment(string name, IParameterType type, object? defaultValue)
            {
                var val = ToCamelCase(name);
                if (defaultValue != null)
                {
                    val += $".GetValueOrDefault({type.Instantiation(defaultValue)})";
                }

                return $@"
            {name} = {val};";
            }

            public static string HasAttributeOfTypeMethod(string modifiableClassName) => $@"
        public bool HasAttributeOfType(AttributeType type) => {modifiableClassName}.AttributeIsKnown(type);
";

            public static string CreateModifiableMethod(string interfaceName, string modifiableClassName) => $@"
        public {interfaceName} CreateModifiableInstance() => new {modifiableClassName}(this);
";
        }
    }
}
