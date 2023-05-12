using System.Collections.Immutable;

namespace Bearded.TD.Game.GameLoop;

sealed record ChapterRequirements(int ChapterNumber, ImmutableArray<double> Waves);
