using System.IO;

namespace Bearded.TD.Mods
{
    struct ModAwareName
    {
        public string ModName { get; }
        public string Name { get; }

        public ModAwareName(string modName, string name)
        {
            ModName = modName;
            Name = name;
        }

        public static ModAwareName FromNameInMod(string name, ModMetadata mod)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidDataException("Name may not be null, empty or whitespace.");

            var components = name.Split('.');
            
            if (components.Length == 1)
                return new ModAwareName(mod.Id, name);

            if (components.Length == 2)
                return new ModAwareName(components[0], components[1]);

            throw new InvalidDataException($"Name may not contain more than one . ({name})");
        }
        
        public override string ToString()
            => $"{ModName}.{Name}";
    }
}