using System.Collections.Immutable;
using System.Linq;
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


        context.RegisterSourceOutput(generatorContexts.Combine(serializerConverters), execute);
    }

    private static bool isConverterCandidate(SyntaxNode node)
    {
        return node is FieldDeclarationSyntax
            {
                Modifiers: var mods,
                AttributeLists.Count: > 0,
            }
            && mods.Any(static m => m.IsKind(SyntaxKind.PublicKeyword))
            && mods.Any(static m => m.IsKind(SyntaxKind.StaticKeyword));
    }

    private static SerializerConverterInfo? getConverterContext(GeneratorAttributeSyntaxContext context)
    {
        var fieldSyntax = (FieldDeclarationSyntax)context.TargetNode;

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
        var converterName = fieldSyntax.Declaration.Variables.First().Identifier.Text;

        return new SerializerConverterInfo(
            ConverterName: converterName,
            DeserializedType: new UsedType(
                deserializedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                deserializedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                ),
            SerializedType: new UsedType(
                serializedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                serializedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            )
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

        if (ModelExtensions.GetDeclaredSymbol(semanticModel, classSyntax) is not INamedTypeSymbol classSymbol)
            return null;

        var parameters = methodSyntax.ParameterList.Parameters
            .Select(p =>
            {
                var type = p.Type;
                var typeSymbol = semanticModel.GetTypeInfo(type!).Type!;
                var shortTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return new Parameter(
                    p.Identifier.Text,
                    new UsedType(
                        ShortName: shortTypeName,
                        FullName: fullTypeName
                    )
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
