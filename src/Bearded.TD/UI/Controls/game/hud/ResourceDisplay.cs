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

    public void Initialize(GameInstance game)
    {
        var faction = game.Me.Faction;
        faction.TryGetBehaviorIncludingAncestors(out resources);

        CurrentScrap = game.Meta.Events
            .Observe<ResourcesChanged<Scrap>>()
            .Where(e => e.Resources == resources)
            .BindDisplayOnly(e => e.NewAmount, out _);

        CurrentCoreEnergy = game.Meta.Events
            .Observe<ResourcesChanged<CoreEnergy>>()
            .Where(e => e.Resources == resources)
            .BindDisplayOnly(e => e.NewAmount, out _);
    }
}
