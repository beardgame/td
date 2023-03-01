using System;

namespace Bearded.TD.Content.Mods;

sealed record ModLoadingError(string Path, Exception Exception);
