using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Content.Mods
{
    class ComponentOwnerProxyBlueprintResolver : ModAwareDependencyResolver<IComponentOwnerBlueprint>
    {
        private List<ComponentOwnerBlueprintProxy> currentProxies = new List<ComponentOwnerBlueprintProxy>();

        public ComponentOwnerProxyBlueprintResolver(ModMetadata thisMod, IEnumerable<Mod> otherMods)
            : base(thisMod, otherMods)
        {
        }

        public List<ComponentOwnerBlueprintProxy> GetAndResetCurrentProxies()
        {
            var proxies = currentProxies;
            currentProxies = new List<ComponentOwnerBlueprintProxy>();
            return proxies;
        }

        protected override IComponentOwnerBlueprint GetDependencyFromThisMod(ModAwareId id)
        {
            // could cache these by id, but they are small objects, and are unlikely to be reused much it at all
            var proxy = new ComponentOwnerBlueprintProxy(id);

            currentProxies.Add(proxy);

            return proxy;
        }

        protected override IComponentOwnerBlueprint GetDependencyFromOtherMod(Mod mod, ModAwareId id)
        {
            return mod.Blueprints.ComponentOwners[id];
        }
    }
}
