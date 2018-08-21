using System.IO;

namespace Bearded.TD.Mods
{
    struct ModAwareId
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
        
        public override string ToString()
            => $"{ModId}.{Id}";
    }
}