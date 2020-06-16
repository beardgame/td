using System;
using System.IO;

namespace Bearded.TD.Content.Mods
{
    readonly struct ModAwareId : IEquatable<ModAwareId>
    {
        public string ModId { get; }
        public string Id { get; }

        public ModAwareId(string modId, string id)
        {
            ModId = modId;
            Id = id;
        }

        public static ModAwareId FromNameInMod(string name, ModMetadata mod)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDataException("Id may not be null, empty or whitespace.");

            var components = name.Split('.');

            switch (components.Length)
            {
                case 1:
                    return new ModAwareId(mod.Id, name);
                case 2:
                    return new ModAwareId(components[0], components[1]);
                default:
                    throw new InvalidDataException($"Id may not contain more than one . ({name})");
            }
        }

        public bool Equals(ModAwareId other) => ModId == other.ModId && Id == other.Id;

        public override bool Equals(object? obj) => obj is ModAwareId other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (ModId.GetHashCode() * 397) ^ Id.GetHashCode();
            }
        }

        public override string ToString() => $"{ModId}.{Id}";

        public static bool operator ==(ModAwareId left, ModAwareId right) => left.Equals(right);

        public static bool operator !=(ModAwareId left, ModAwareId right) => !(left == right);

        public static ModAwareId ForDefaultMod(string id) => new ModAwareId(Constants.Content.DefaultModId, id);
    }
}
