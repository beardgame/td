using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Generators.Types;
using Microsoft.CodeAnalysis;
using static Bearded.TD.Generators.SourceTemplates;

namespace Bearded.TD.Generators.Serializers;

public partial class CommandGenerator
{
    private const string serializerBufferStreamInterfaceName
        = "global::Bearded.TD.Shared.Commands.ISerializerBufferStream";
    private const string serializerContextTypeName = "global::Bearded.TD.Game.GameInstance";

    private const string converterSerializeMethodName = "Serialize";
    private const string converterDeserializeMethodName = "Deserialize";

    private static readonly ImmutableArray<string> defaultNameSpaces = ImmutableArray.Create<string>(
        "Bearded.TD.Commands",
        "Bearded.TD.Commands.Serialization",
        "Bearded.TD.Networking.Serialization",
        "Bearded.TD.Utilities",
        "Bearded.Utilities",
        "Bearded.Utilities.SpaceTime"
    );

    private record struct CommandInfoWithConverters(
        CommandInfo Command,
        EquatableArray<SerializerConverterInfo> Converters,
        EquatableArray<SerializerInfo> Serializers);

    private record struct SerializerConverterInfo(
        TypeName ConverterType, string ConverterMemberName, TypeName DeserializedType, TypeName SerializedType);

    private record struct SerializerInfo(
        TypeName SerializerType, string SerializerMemberName, TypeName SerializedType);

    private record struct CommandInfo(
        string Namespace,
        string ClassName,
        EquatableArray<Parameter> Parameters
    );

    private record struct Parameter(string Name, TypeName Type);

    private static void execute(
        SourceProductionContext context,
        CommandInfoWithConverters info)
    {
        var (command, converters, serializers) = info;

        var aliases = new TypeAliases();

        aliases.AddRange(command.Parameters.Select(p => p.Type));
        aliases.AddRange(converters.Select(c => c.ConverterType));
        aliases.AddRange(converters.Select(c => c.SerializedType));
        aliases.AddRange(serializers.Select(c => c.SerializerType));

        var parametersList = string.Join(", ", command.Parameters.Select(p => p.Name));
        var argumentList = string.Join(", ", command.Parameters.Select(p => $"{aliases[p.Type]} {p.Name}"));

        var source =
$$"""
{{FileHeader(defaultNameSpaces, aliases)}}

namespace {{command.Namespace}};

static partial class {{command.ClassName}}
{
    public static ISerializableCommand<GameInstance> Command({{argumentList}})
        => new Implementation({{parametersList}});

    private sealed class Implementation({{argumentList}})
        : ISerializableCommand<GameInstance>
    {
        public void Execute()
        {
            execute({{parametersList}});
        }

        public ICommandSerializer<GameInstance> Serializer
            => new Serializer({{parametersList}});
    }

    private sealed class Serializer() : ICommandSerializer<GameInstance>
    {
        {{foreachParam(2, p => $"private {serializedTypeName(p)} {p.Name};")}}

        public Serializer({{argumentList}}) : this()
        {
            {{foreachParam(3, p => $"this.{p.Name} = {serializeConvert(p)};")}}
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(
                {{foreachParam(4, deserializeConvert, ",\n")}}
            );
        }

        public void Serialize(INetBufferStream stream)
        {
            {{foreachParam(3, serialize)}}
        }
    }
}
""";

        source = CleanWhiteSpace(source);

        context.AddSource($"{command.ClassName}_Implementation.g.cs", source);

        return;

        string serialize(Parameter p)
        {
            var parameterType = p.Type;

            foreach (var converter in converters)
            {
                if (converter.DeserializedType.FullName == parameterType.FullName)
                {
                    parameterType = converter.SerializedType;
                    break;
                }
            }

            foreach (var serializer in serializers)
            {
                if (serializer.SerializedType.FullName == parameterType.FullName)
                {
                    return
                        aliases[serializer.SerializerType] +
                        $".{serializer.SerializerMemberName}(stream, ref {p.Name});";
                }
            }

            return $"stream.Serialize(ref {p.Name});";
        }

        ShortTypeName serializedTypeName(Parameter parameter)
        {
            foreach (var converter in converters)
            {
                if (converter.DeserializedType.FullName == parameter.Type.FullName)
                {
                    return aliases[converter.SerializedType];
                }
            }

            return aliases[parameter.Type];
        }

        string serializeConvert(Parameter parameter)
        {
            foreach (var converter in converters)
            {
                if (converter.DeserializedType.FullName == parameter.Type.FullName)
                {
                    return
                        aliases[converter.ConverterType] +
                        $".{converter.ConverterMemberName}" +
                        $".{converterSerializeMethodName}({parameter.Name})";
                }
            }

            return $"{parameter.Name}";
        }

        string deserializeConvert(Parameter parameter)
        {
            foreach (var converter in converters)
            {
                if (converter.DeserializedType.FullName == parameter.Type.FullName)
                {
                    return
                        aliases[converter.ConverterType] +
                        $".{converter.ConverterMemberName}" +
                        $".{converterDeserializeMethodName}({parameter.Name}, game)";
                }
            }

            return $"{parameter.Name}";
        }

        string foreachParam(int indents, Func<Parameter, string> template, string separator = "\n")
        {
            return Foreach(indents, command.Parameters, template, separator);
        }
    }
}
