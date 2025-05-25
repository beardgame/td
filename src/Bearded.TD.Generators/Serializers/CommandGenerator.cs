using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Bearded.TD.Generators.Types;
using Bearded.TD.Shared.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bearded.TD.Generators.Serializers;

[Generator]
public partial class CommandGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var serializerConverters = context.SyntaxProvider
            .ForAttributeWithMetadataName(typeof(SerializerConverterAttribute).FullName!,
                predicate: static (ctx, _) => isConverterCandidate(ctx),
                transform: static (ctx, _) => getConverterContext(ctx)
            )
            .WhereNotNull()
            .Collect();

        var generatorContexts = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                typeof(GenerateCommandAttribute).FullName!,
                predicate: static (ctx, _) => isCandidateCommandClass(ctx),
                transform: static (ctx, _) => getGeneratorContext(ctx)
            )
            .WhereNotNull();

        var contextsAndConverters = generatorContexts
            .Combine(serializerConverters)
            .Select(commandWithUsedConverters);

        context.RegisterSourceOutput(contextsAndConverters, execute);
    }

    private CommandInfoWithConverters commandWithUsedConverters(
        (CommandInfo, ImmutableArray<SerializerConverterInfo>) info, CancellationToken _)
    {
        var (command, converters) = info;

        var usedConverters = converters
            .Where(converter => command.Parameters.Any(parameter => parameter.Type == converter.DeserializedType))
            .ToImmutableArray();

        return new CommandInfoWithConverters(command, usedConverters);
    }

    private static bool isConverterCandidate(SyntaxNode node)
    {
        return node.Parent?.Parent is FieldDeclarationSyntax
            {
                Modifiers: var mods,
                AttributeLists.Count: > 0,
            }
            && mods.Any(static m => m.IsKind(SyntaxKind.PublicKeyword))
            && mods.Any(static m => m.IsKind(SyntaxKind.StaticKeyword));
    }

    private static SerializerConverterInfo? getConverterContext(GeneratorAttributeSyntaxContext context)
    {
        var fieldSyntax = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;

        var semanticModel = context.SemanticModel;

        var fieldType = semanticModel.GetTypeInfo(fieldSyntax.Declaration.Type).Type!;
        if (fieldType is not INamedTypeSymbol fieldTypeSymbol)
            return null;

        var typeArguments = fieldTypeSymbol.TypeArguments;
        if (typeArguments.Length != 3)
            return null;

        var contextType = typeArguments[2];

        if (contextType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != serializerContextTypeName)
            return null;

        var deserializedType = typeArguments[0];
        var serializedType = typeArguments[1];

        var typeSyntax = (TypeDeclarationSyntax)fieldSyntax.Parent!;

        if (semanticModel.GetDeclaredSymbol(typeSyntax) is not { } typeSymbol)
            return null;

        var converterName = fieldSyntax.Declaration.Variables.First().Identifier.Text;

        return new SerializerConverterInfo(
            ConverterType: TypeName.From(typeSymbol),
            ConverterMemberName: converterName,
            DeserializedType: TypeName.From(deserializedType),
            SerializedType: TypeName.From(serializedType)
        );
    }

    private static bool isCandidateCommandClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax
            {
                Modifiers: var mods,
            }
            && mods.Any(static m => m.IsKind(SyntaxKind.PartialKeyword))
            && mods.Any(static m => m.IsKind(SyntaxKind.StaticKeyword));
    }

    private static CommandInfo? getGeneratorContext(GeneratorAttributeSyntaxContext context)
    {
        var classSyntax = (ClassDeclarationSyntax)context.TargetNode;
        var methodSyntax = classSyntax.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == "execute");

        if (methodSyntax == null)
            return null;

        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(classSyntax) is not { } classSymbol)
            return null;

        var parameters = methodSyntax.ParameterList.Parameters
            .Select(p =>
            {
                var type = p.Type;
                var typeSymbol = semanticModel.GetTypeInfo(type!).Type!;
                return new Parameter(
                    p.Identifier.Text,
                    TypeName.From(typeSymbol)
                );
            })
            .ToImmutableArray()
            .AsEquatableArray();

        return new CommandInfo(
            getNamespace(classSyntax),
            classSymbol.Name,
            parameters
        );
    }


    private static string getNamespace(BaseTypeDeclarationSyntax syntax)
    {
        var potentialNamespaceParent = syntax.Parent;

        while (potentialNamespaceParent is not null
               and not NamespaceDeclarationSyntax
               and not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is not BaseNamespaceDeclarationSyntax namespaceNode)
            return "";

        var nameSpace = namespaceNode.Name.ToString();

        while (namespaceNode.Parent is NamespaceDeclarationSyntax parent)
        {
            namespaceNode = parent;
            nameSpace = $"{namespaceNode.Name}.{nameSpace}";
        }

        return nameSpace;
    }

}
