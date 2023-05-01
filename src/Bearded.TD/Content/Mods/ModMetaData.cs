using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;

namespace Bearded.TD.Content.Mods;

sealed class ModMetadata
{
    public string Name { get; }
    public string Id { get; }
    private ModFlags flags { get; }
    public IReadOnlyCollection<ModDependency> Dependencies { get; }
    public DirectoryInfo Directory { get; }

    public bool Visible => !flags.HasFlag(ModFlags.AlwaysHidden);

    public ModMetadata(Metadata meta, DirectoryInfo directory)
    {
        Name = meta.Name;
        Id = meta.Id;
        flags = meta.Flags?.Aggregate(ModFlags.None, (f, flag) => f | flag) ?? ModFlags.None;
        Dependencies = (meta.Dependencies?.Select(d => new ModDependency(d)).ToList() ?? new List<ModDependency>()).AsReadOnly();
        Directory = directory;
    }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Id) &&
        !string.IsNullOrWhiteSpace(Name) &&
        Dependencies.All(d => d.IsValid);

    public ModForLoading PrepareForLoading() => new ModForLoading(this);
}

[Flags]
enum ModFlags
{
    None = 0,
    AlwaysHidden = 1,
}
