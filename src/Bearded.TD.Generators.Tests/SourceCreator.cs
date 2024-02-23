using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bearded.TD.Shared.Proxies;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bearded.TD.Generators.Tests
{
    static class SourceCreator
    {
        private static readonly string dotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        public static readonly ImmutableArray<MetadataReference> References = ImmutableArray.Create<MetadataReference>(
            // .NET assemblies are finicky and need to be loaded in a special way.
            MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "mscorlib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Core.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Private.CoreLib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(dotNetAssemblyPath, "System.Runtime.dll")),

            // JetBrains
            MetadataReference.CreateFromFile(typeof(UsedImplicitlyAttribute).Assembly.Location),

            // Bearded
            MetadataReference.CreateFromFile(typeof(AutomaticProxyAttribute).Assembly.Location)
        );

        public static async Task<SyntaxTree> SyntaxTreeFromRelativeFile(
            string relativePath, [CallerFilePath] string sourceFilePath = "")
        {
            var dir = Path.GetDirectoryName(sourceFilePath)
                ?? throw new InvalidOperationException($"Could not extract source file path from {sourceFilePath}");
            var toRead = Path.Combine(dir, relativePath);
            var source = await File.ReadAllTextAsync(toRead);
            return SyntaxFactory.ParseSyntaxTree(source);
        }
    }
}
