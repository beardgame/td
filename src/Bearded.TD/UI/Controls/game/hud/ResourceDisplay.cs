using System;
using System.Reactive.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ResourceDisplay
{
    public IReadonlyBinding<Resource<Scrap>> CurrentScrap { get; private set; } = null!;
    public IReadonlyBinding<Resource<CoreEnergy>> CurrentCoreEnergy { get; private set; } = null!;

    public IReadonlyBinding<Resource<Scrap>> ScrapLeftThisWave { get; private set; } = null!;
    public IReadonlyBinding<Resource<CoreEnergy>> CoreEnergyLeftThisWave { get; private set; } = null!;

    public CoreEnergyExchange Exchange { get; } = new();

    public void Initialize(GameInstance game)
    {
        var exchange = new ResourceExchangeObserver(game);
        ScrapLeftThisWave = exchange.ScrapLeftThisWave;
        CoreEnergyLeftThisWave = exchange.CoreEnergyLeftThisWave;

        Exchange.Initialize(game, exchange);

        var faction = game.Me.Faction;
        faction.TryGetBehaviorIncludingAncestors(out FactionResources? resources);

        CurrentScrap = observe<Scrap>(game, resources).BindDisplayOnly(out _);
        CurrentCoreEnergy = observe<CoreEnergy>(game, resources).BindDisplayOnly(out _);
    }

    private static IObservable<Resource<T>> observe<T>(GameInstance game, FactionResources? resources)
        where T : IResourceType
    {
        return game.Meta.Events.Observe<ResourcesChanged<T>>()
            .Where(e => e.Resources == resources)
            .StartWith(new ResourcesChanged<T>(null!, resources?.GetCurrent<T>() ?? Resource<T>.Zero))
            .Select(e => e.NewAmount);
    }
}
