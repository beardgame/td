using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class ComponentParameterTemplateConverter : JsonConverter<object>
    {
        private readonly Type interfaceType;
        private readonly Type templateType;

        public ComponentParameterTemplateConverter(Type interfaceType, Type templateType)
        {
            this.interfaceType = interfaceType;
            this.templateType = templateType;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new InvalidOperationException();
        }

        public override object? Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(ref reader, templateType, options);
        }

        public override bool CanConvert(Type objectType) => objectType == interfaceType;
    }
}
