using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Bearded.TD.Content.Mods;

readonly struct ModAwareId : IEquatable<ModAwareId>
{
    private const string invalidString = "invalid";

    public static ModAwareId Invalid { get; } = new();

    public string? ModId { get; }
    public string? Id { get; }

    public ModAwareId(string modId, string id)
    {
        if (string.IsNullOrEmpty(modId))
        {
            throw new ArgumentException("Mod id must be non-null and non-empty", nameof(modId));
        }
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id must be non-null and non-empty", nameof(id));
        }

        ModId = modId;
        Id = id;
    }

    public static ModAwareId FromNameInMod(string name, ModMetadata mod)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Id must be non-null and non-empty", nameof(name));
        }

        if (name == invalidString)
        {
            return Invalid;
        }

        var components = name.Split('.');

        return components.Length switch
        {
            1 => new ModAwareId(mod.Id, name),
            2 => new ModAwareId(components[0], components[1]),
            _ => throw new InvalidDataException($"Id may not contain more than one . ({name})")
        };
    }

    public static ModAwareId FromFullySpecified(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Id must be non-null and non-empty", nameof(name));
        }

        if (name == invalidString)
        {
            return Invalid;
        }

        var components = name.Split('.');
        if (components.Length != 2)
        {
            throw new InvalidDataException($"Id must contain exactly 1 . ({name})");
        }

        return new ModAwareId(components[0], components[1]);
    }

    [MemberNotNullWhen(true, nameof(ModId), nameof(Id))]
    // ModId == null implies Id == null
    public bool IsValid => ModId != null;

    public bool Equals(ModAwareId other) => ModId == other.ModId && Id == other.Id;

    public override bool Equals(object? obj) => obj is ModAwareId other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return ((ModId?.GetHashCode() ?? 0) * 397) ^ (Id?.GetHashCode() ?? 0);
        }
    }

    public override string ToString() => IsValid ? $"{ModId}.{Id}" : invalidString;

    public static bool operator ==(ModAwareId left, ModAwareId right) => left.Equals(right);

    public static bool operator !=(ModAwareId left, ModAwareId right) => !(left == right);

    public static ModAwareId ForDefaultMod(string id) => new(Constants.Content.DefaultModId, id);
}
