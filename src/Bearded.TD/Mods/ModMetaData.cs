using System.Collections.Generic;

namespace Bearded.TD.Mods
{
    sealed class ModMetadata
    {
        public string Name { get; }
        public string Id { get; }
        public IReadOnlyCollection<ModDependency> Dependencies { get; }

        public ModForLoading PrepareForLoading() => new ModForLoading(this);
    }
}
