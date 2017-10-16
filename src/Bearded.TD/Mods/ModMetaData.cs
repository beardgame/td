using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Mods.Serialization.Models;

namespace Bearded.TD.Mods
{
    sealed class ModMetadata
    {
        public string Name { get; }
        public string Id { get; }
        public IReadOnlyCollection<ModDependency> Dependencies { get; }
        public DirectoryInfo Directory { get; }

        public ModMetadata(Metadata meta, DirectoryInfo directory)
        {
            Name = meta.Name;
            Id = meta.Id;
            Dependencies = meta.Dependencies.Select(d => new ModDependency(d)).ToList();
            Directory = directory;
        }

        public ModForLoading PrepareForLoading() => new ModForLoading(this);
    }
}
