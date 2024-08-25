using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Bearded.TD.Testing")]
[assembly:InternalsVisibleTo("Bearded.TD.Tests")]
[assembly:InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Bearded.TD;

static class Config
{
    public const string BaseVersionString = "0.30";

#if DEBUG
    private const string versionSuffix = "-dev";
#else
        private const string versionSuffix = "";
#endif

    public static string VersionString { get; } = BaseVersionString + versionSuffix;
}
