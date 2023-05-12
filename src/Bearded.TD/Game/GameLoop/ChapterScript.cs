using System.Collections.Immutable;

namespace Bearded.TD.Game.GameLoop;

sealed record ChapterScript(
    int ChapterNumber,
    ImmutableArray<WaveDescription> Waves,
    ElementalTheme Elements)
{
    public int WaveCount => Waves.Length;
}
