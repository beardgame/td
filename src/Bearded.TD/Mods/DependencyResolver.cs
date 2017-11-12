using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bearded.TD.Mods
{
    sealed class DependencyResolver<T>
    {
        private readonly ModMetadata thisMod;
        private readonly Dictionary<string, Mod> mods;

        public DependencyResolver(ModMetadata thisMod, IEnumerable<Mod> otherMods)
        {
            this.thisMod = thisMod;
            mods = otherMods.ToDictionary(m => m.Id);
        }

        public T Resolve(string name)
            => Resolve(ModAwareName.FromNameInMod(name, thisMod));

        public T Resolve(ModAwareName name)
        {
            if (name.ModName == thisMod.Name)
            {
                
            }

            if (mods.TryGetValue(name.ModName, out var mod))
            {
                
            }
            
            throw new InvalidDataException($"Unknown mod in identifier {name}");
        }
    }
}
