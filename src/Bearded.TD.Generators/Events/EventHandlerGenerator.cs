using System.Collections.Generic;
using Bearded.TD.Generators.Types;
using Bearded.TD.Shared.Events;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using static Bearded.TD.Generators.SourceTemplates;

namespace Bearded.TD.Generators.Events;

[Generator]
public class EventHandlerGenerator : IIncrementalGenerator
{
    private const string eventBaseTypeName =
        "global::Bearded.TD.Game.Simulation.GameObjects.IComponentEvent";

    private static readonly ImmutableArray<string> defaultNamespaces =
        ImmutableArray.Create(
            "Bearded.TD.Shared.Events"
        );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var handlerMethods =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                    typeof(HandlerAttribute).FullName!,
                    predicate: static (node, _) => isValidHandlerMethod(node),
                    transform: static (ctx, _) => handlerMethodFromContext(ctx))
                .WhereNotNull()
                .Collect()
                .SelectMany(static (methods, _) => groupByClass(methods));

        context.RegisterSourceOutput(handlerMethods, generateSource);
    }

    private static bool isValidHandlerMethod(SyntaxNode node)
    {
        // Check the method
        if (node is not MethodDeclarationSyntax method ||
            method.ParameterList.Parameters.Count != 1)
        {
            return false;
        }

        // Check the parent class
        if (method.Parent is not ClassDeclarationSyntax { Modifiers: var mods } ||
            !mods.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return false;
        }

        return true;
    }

    private static HandlerMethod? handlerMethodFromContext(GeneratorAttributeSyntaxContext context)
    {
        var method = (IMethodSymbol) context.TargetSymbol;
        var semanticModel = context.SemanticModel;

        var methodName = method.Name;
        var eventParameter = method.Parameters[0];
        if (eventParameter.Type.AllInterfaces
            .Select(TypeName.From)
            .All(name => name.FullName.Name != eventBaseTypeName))
        {
            return null;
        }

        var eventType = TypeName.From(eventParameter.Type);

        if (semanticModel.GetDeclaredSymbol(context.TargetNode.Parent!) is not ITypeSymbol parentClass)
        {
            return null;
        }

        var componentType = TypeName.From(parentClass);
        var componentNamespace = Namespace.From(parentClass.ContainingNamespace);

        return new HandlerMethod(componentType, componentNamespace, methodName, eventType);
    }

    private static IEnumerable<ClassWithHandlers> groupByClass(ImmutableArray<HandlerMethod> methods)
    {
        return methods.GroupBy(m => m.ComponentType)
            .Select(group => new ClassWithHandlers(
                group.Key,
                group.First().Namespace,
                group.OrderBy(m => m.EventType.ShortName.Name).ToImmutableArray()))
            .OrderBy(c => c.Name.ShortName.Name);
    }

    private static void generateSource(
        SourceProductionContext context, ClassWithHandlers classWithHandlers)
    {
        var methods = classWithHandlers.Handlers;

        var aliases = new TypeAliases();
        aliases.AddRange(methods.Select(m => m.EventType));

        var source =
$$"""
{{FileHeader(defaultNamespaces, aliases)}}

{{classWithHandlers.Namespace.AsFileScopedDeclaration()}}

partial class {{classWithHandlers.Name.ShortName}} :
    {{Foreach(1, methods, m => $"IListener<{aliases[m.EventType]}>", "," + Strings.NewLine)}}
{
    protected override void RegisterHandlers()
    {
        {{Foreach(2, methods, subscribe)}}
    }

    protected override void UnregisterHandlers()
    {
        {{Foreach(2, methods, unsubscribe)}}
    }

{{Foreach(0, methods, handleEvent)}}
}
""";

        source = CleanWhiteSpace(source);

        context.AddSource($"{classWithHandlers.Name.ShortName}.Events.g.cs", source);

        return;

        string subscribe(HandlerMethod method)
        {
            return $"Events.Subscribe<{aliases[method.EventType]}>(this);";
        }

        string unsubscribe(HandlerMethod method)
        {
            return $"Events.Unsubscribe<{aliases[method.EventType]}>(this);";
        }

        string handleEvent(HandlerMethod method)
        {
            return
$$"""
    public void HandleEvent({{aliases[method.EventType]}} e)
    {
        {{method.MethodName}}(e);
    }

""";
        }
    }

    private record struct ClassWithHandlers(
        TypeName Name,
        Namespace Namespace,
        EquatableArray<HandlerMethod> Handlers);

    private record struct HandlerMethod(
        TypeName ComponentType,
        Namespace Namespace,
        string MethodName,
        TypeName EventType);
}
