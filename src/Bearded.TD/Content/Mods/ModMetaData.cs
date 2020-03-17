using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;

namespace Bearded.TD.Content.Mods
{
    sealed class ModMetadata
    {
        public string Name { get; }
        public string Id { get; }
        public IReadOnlyCollection<ModDependency> Dependencies { get; }
        public DirectoryInfo Directory { get; }

        public ModMetadata(Metadata meta, DirectoryInfo directory)
        {
            Name = meta.Name!;
            Id = meta.Id!;
            Dependencies = (meta.Dependencies?.Select(d => new ModDependency(d)).ToList() ?? new List<ModDependency>()).AsReadOnly();
            Directory = directory;
        }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Id) &&
            !string.IsNullOrWhiteSpace(Name) &&
            Dependencies.All(d => d.IsValid);

        public ModForLoading PrepareForLoading() => new ModForLoading(this);
    }
}
