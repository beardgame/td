using System.Reactive.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ResourceDisplay
{
    private FactionResources? resources;

    public IReadonlyBinding<Resource<Scrap>> CurrentScrap { get; private set; } = null!;
    public IReadonlyBinding<Resource<CoreEnergy>> CurrentCoreEnergy { get; private set; } = null!;

    public CoreEnergyExchange Exchange { get; } = new();

    public void Initialize(GameInstance game)
    {
        var faction = game.Me.Faction;
        faction.TryGetBehaviorIncludingAncestors(out resources);

        CurrentScrap = createBindingFor<Scrap>(game);
        CurrentCoreEnergy = createBindingFor<CoreEnergy>(game);

        Exchange.Initialize(game);
    }

    private IReadonlyBinding<Resource<T>> createBindingFor<T>(GameInstance game)
        where T : IResourceType
    {
        return game.Meta.Events
            .Observe<ResourcesChanged<T>>()
            .Where(e => e.Resources == resources)
            .StartWith(new ResourcesChanged<T>(null!, resources?.GetCurrent<T>() ?? Resource<T>.Zero))
            .BindDisplayOnly(e => e.NewAmount, out _);
    }
}
