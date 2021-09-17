using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bearded.TD.Content.Mods
{
    abstract class ModAwareDependencyResolver<T> : IDependencyResolver<T>
    {
        private readonly ModMetadata thisMod;
        private readonly Dictionary<string, Mod> mods;

        protected ModAwareDependencyResolver(ModMetadata thisMod, IEnumerable<Mod> otherMods)
        {
            this.thisMod = thisMod;
            mods = otherMods.ToDictionary(m => m.Id);
        }

        public T Resolve(string id)
            => Resolve(ModAwareId.FromNameInMod(id, thisMod));

        public T Resolve(ModAwareId id)
        {
            try
            {
                if (id.ModId == null || id.ModId == thisMod.Id)
                {
                    return GetDependencyFromThisMod(id);
                }

                if (mods.TryGetValue(id.ModId, out var mod))
                {
                    return GetDependencyFromOtherMod(mod, id);
                }
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Failed to find {typeof(T).Name} with id \"{id}\".", e);
            }

            throw new InvalidDataException($"Unknown mod in identifier {id}");
        }

        protected abstract T GetDependencyFromThisMod(ModAwareId id);
        protected abstract T GetDependencyFromOtherMod(Mod mod, ModAwareId id);
    }
}
