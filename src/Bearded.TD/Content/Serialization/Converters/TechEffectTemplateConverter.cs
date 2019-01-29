using System;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class TechEffectTemplateConverter : JsonConverter
    {
        private readonly Type interfaceType;
        private readonly Type templateType;

        public TechEffectTemplateConverter(Type interfaceType, Type templateType)
        {
            this.interfaceType = interfaceType;
            this.templateType = templateType;
        }

        public override bool CanWrite { get; } = false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, templateType);
        }

        public override bool CanConvert(Type objectType) => objectType == interfaceType;
    }
}