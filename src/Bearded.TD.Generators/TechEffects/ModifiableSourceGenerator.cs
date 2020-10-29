using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bearded.TD.Generators.TechEffects
{
    sealed class ModifiableSourceGenerator
    {
        public static string GenerateFor(ParametersTemplateDefinition definition)
        {
            return new ModifiableSourceGenerator()
                .addFileTop(definition.Namespace, definition.ModifiableName, definition.InterfaceName)
                .addImmutableProperties(
                    definition.Properties.Select(property => (Type: $"{property.Type}", PropertyName: property.Name)))
                .addConstructor(definition.InterfaceName, definition.ModifiableName)
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
            IEnumerable<(string Type, string PropertyName)> properties)
        {
            foreach (var (type, name) in properties)
            {
                sb.Append(Templates.ImmutableProperty(type, name));
            }
            return this;
        }

        private ModifiableSourceGenerator addConstructor(string interfaceName, string className)
        {
            sb.Append(Templates.Constructor(interfaceName, className));
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

            public static string Constructor(string interfaceName, string className) => $@"
        public {className}({interfaceName} template)
        {{
            this.template = template;
        }}
";

            public static string CreateModifiableMethod(string interfaceName, string className) => $@"
        public {interfaceName} CreateModifiableInstance() => new {className}(template);
";
        }
    }
}
