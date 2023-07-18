using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Content.Mods;

sealed class GameObjectProxyBlueprintResolver : ModAwareDependencyResolver<IGameObjectBlueprint>
{
    private List<GameObjectBlueprintProxy> currentProxies = new List<GameObjectBlueprintProxy>();

    public GameObjectProxyBlueprintResolver(ModMetadata thisMod, IEnumerable<Mod> otherMods)
        : base(thisMod, otherMods)
    {
    }

    public List<GameObjectBlueprintProxy> GetAndResetCurrentProxies()
    {
        var proxies = currentProxies;
        currentProxies = new List<GameObjectBlueprintProxy>();
        return proxies;
    }

    protected override IGameObjectBlueprint GetDependencyFromThisMod(ModAwareId id)
    {
        // could cache these by id, but they are small objects, and are unlikely to be reused much it at all
        var proxy = new GameObjectBlueprintProxy(id);

        currentProxies.Add(proxy);

        return proxy;
    }

    protected override IGameObjectBlueprint GetDependencyFromOtherMod(Mod mod, ModAwareId id)
    {
        return mod.Blueprints.GameObjects[id];
    }
}
