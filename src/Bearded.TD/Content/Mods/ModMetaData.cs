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
        Name = meta.Name ?? throw new ArgumentException("Mod must always have a name", nameof(meta));
        Id = meta.Id! ?? throw new ArgumentException("Mod must always have an ID", nameof(meta));
        flags = meta.Flags?.Aggregate(ModFlags.None, (f, flag) => f | flag) ?? ModFlags.None;
        Dependencies = (meta.Dependencies?.Select(d => new ModDependency(d)).ToList() ?? []).AsReadOnly();
        Directory = directory;
    }
}

[Flags]
enum ModFlags
{
    None = 0,
    AlwaysHidden = 1,
}
