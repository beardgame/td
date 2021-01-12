using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class DependencyConverterFactory : JsonConverterFactory
    {
        private readonly Dictionary<Type, object> resolversByType = new();
        private readonly Dictionary<Type, JsonConverter> cache = new();

        public void RegisterDependencyResolver<T>(IDependencyResolver<T> resolver)
        {
            resolversByType[typeof(T)] = resolver;
        }

        public void UnregisterDependencyResolver<T>()
        {
            resolversByType.Remove(typeof(T));
            cache.Remove(typeof(T));
        }

        public override bool CanConvert(Type typeToConvert) => resolversByType.ContainsKey(typeToConvert);

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (cache.TryGetValue(typeToConvert, out var converter))
            {
                return converter;
            }

            if (!resolversByType.TryGetValue(typeToConvert, out var resolver))
            {
                throw new ArgumentException($"Cannot create converter for unsupported type {typeToConvert.Name}");
            }

            converter = (JsonConverter) Activator.CreateInstance(
                typeof(DependencyConverter<>).MakeGenericType(typeToConvert),
                bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new[] {resolver},
                culture: null);
            cache[typeToConvert] = converter;

            return converter;
        }

        private sealed class DependencyConverter<T> : JsonConverterBase<T>
        {
            private readonly IDependencyResolver<T> dependencyResolver;

            public DependencyConverter(IDependencyResolver<T> dependencyResolver)
            {
                this.dependencyResolver = dependencyResolver;
            }

            protected override T ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException(
                        $"Expected string value, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
                }

                return dependencyResolver.Resolve(reader.GetString()!);
            }
        }
    }
}
