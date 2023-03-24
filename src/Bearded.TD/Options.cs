using CommandLine;
using JetBrains.Annotations;

namespace Bearded.TD;

[UsedImplicitly]
sealed class Options
{
    [Option]
    public Intent Intent { get; set; }
}

enum Intent : byte
{
    None = 0,
    QuickGame,
}
