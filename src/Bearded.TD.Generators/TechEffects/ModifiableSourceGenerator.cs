using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Bearded.TD.Generators.TechEffects.ParametersTemplateDefinition;
using static Bearded.TD.Generators.TechEffects.TechEffectsUtils;

namespace Bearded.TD.Generators.TechEffects
{
    sealed class ModifiableSourceGenerator
    {
        public static string GenerateFor(ParametersTemplateDefinition definition)
        {
            var immutableProperties = definition.Properties.Where(property => !property.IsModifiable);
            var modifiableProperties = definition.Properties.Where(property => property.IsModifiable).ToList();

            return new ModifiableSourceGenerator()
                .addFileTop(definition.Namespace, definition.ModifiableName, definition.InterfaceName)
                .addImmutableProperties(immutableProperties)
                .addModifiableProperties(modifiableProperties)
                .addConstructor(definition.InterfaceName, definition.ModifiableName, modifiableProperties)
                .addStaticConstructor(definition.ModifiableName, modifiableProperties)
                .addCreateModifiableInstanceMethod(definition.InterfaceName, definition.ModifiableName)
                .addFileBottom()
                .build();
        }

        private readonly StringBuilder sb = new StringBuilder();

        private ModifiableSourceGenerator addFileTop(string @namespace, string className, string interfaceName)
        {
            sb.Append(Templates.FileHeader);
            sb.Append(Templates.ClassTop(@namespace, className, interfaceName));
            return this;
        }

        private ModifiableSourceGenerator addImmutableProperties(
            IEnumerable<ParametersPropertyDefinition> properties)
        {
            foreach (var p in properties)
            {
                sb.Append(Templates.ImmutableProperty(p.Type, p.Name));
            }

            return this;
        }

        private ModifiableSourceGenerator addModifiableProperties(IEnumerable<ParametersPropertyDefinition> properties)
        {
            foreach (var p in properties)
            {
                sb.Append(Templates.ModifiableProperty(p.Type, p.Name));
            }

            return this;
        }

        private ModifiableSourceGenerator addConstructor(
            string interfaceName,
            string className,
            IEnumerable<ParametersPropertyDefinition> modifiableParameters)
        {
            sb.Append(Templates.Constructor(interfaceName, className, modifiableParameters));
            return this;
        }

        private ModifiableSourceGenerator addStaticConstructor(
            string className,
            ICollection<ParametersPropertyDefinition> modifiableParameters)
        {
            sb.Append(Templates.StaticConstructor(className, modifiableParameters));
            return this;
        }

        private ModifiableSourceGenerator addCreateModifiableInstanceMethod(
            string interfaceName, string className)
        {
            sb.Append(Templates.CreateModifiableMethod(interfaceName, className));
            return this;
        }

        private ModifiableSourceGenerator addFileBottom()
        {
            sb.Append(Templates.ClassBottom);
            return this;
        }

        private string build() => sb.ToString();

        private static class Templates
        {
            public const string FileHeader = @"
using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
";

            public static string ClassTop(
                string @namespace, string className, string interfaceName) => $@"
namespace {@namespace}
{{
    sealed class {className} : ModifiableBase<{className}>, {interfaceName}
    {{
        private readonly {interfaceName} template;
";

            public const string ClassBottom = @"
    }
}
";

            public static string ImmutableProperty(string type, string name) => $@"
        public {type} {name} => template.{name};";

            public static string ModifiableProperty(string type, string name)
            {
                var fieldName = toCamelCase(name);
                return $@"
        private readonly AttributeWithModifications<{type}> {fieldName};
        public {type} {name} => {fieldName}.Value;
";
            }

            public static string Constructor(
                string interfaceName,
                string className,
                IEnumerable<ParametersPropertyDefinition> modifiableParameters)
            {
                var assignments = string.Join("\n",
                    modifiableParameters.Select(p => constructorAssignment(p.Type, p.Name, p.Converter)));
                return $@"
        public {className}({interfaceName} template)
        {{
            this.template = template;
            {assignments}
        }}
";
            }

            private static string constructorAssignment(string type, string name, string? converter)
            {
                var initialValue = $"template.{name}";
                if (converter != null)
                {
                    initialValue = $"{converter}.ToRaw({initialValue})";
                }

                var fromRawConverter = converter == null ? $"x => ({type}) x" : $"{converter}.ToWrapped";
                var fieldName = toCamelCase(name);

                return $@"
            {fieldName} = new AttributeWithModifications<{type}>(
                {initialValue},
                {fromRawConverter});
";
            }

            public static string StaticConstructor(
                string className, IEnumerable<ParametersPropertyDefinition> modifiableParameters)
            {
                var attributeTuples = string.Join(",",
                    modifiableParameters.Select(p => staticConstructorAttributeTuple(p.AttributeType, p.Name)));
                return $@"
        static {className}()
        {{
            InitializeAttributes(
                new (AttributeType Type, Func<{className}, IAttributeWithModifications> Getter)[]{{
                    {attributeTuples}
                }}.ToLookup(tuple => tuple.Type, tuple => tuple.Getter));
        }}
";
            }

            private static string staticConstructorAttributeTuple(string attributeType, string propertyName)
            {
                return $@"
                    ({attributeType}, instance => instance.{toCamelCase(propertyName)})";
            }

            public static string CreateModifiableMethod(string interfaceName, string className) => $@"
        public {interfaceName} CreateModifiableInstance() => new {className}(template);
";
        }
    }
}
