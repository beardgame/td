using System.Collections.Immutable;

namespace Bearded.TD.Utilities.Console;

sealed record CommandParameters(ImmutableArray<string> Args);