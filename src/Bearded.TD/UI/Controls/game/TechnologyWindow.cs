using System;
using System.Diagnostics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class TechnologyWindow : IListener<TechnologyTokenAwarded>, IListener<TechnologyTokenConsumed>
{
    private GameInstance game = null!;
    private FactionTechnology factionTechnology = null!;
    private Binding<bool> windowVisibility = null!;

    public TooltipFactory TooltipFactory { get; private set; } = null!;
    public TechTree TechTree { get; private set; } = null!;
    public Binding<bool> CanUnlockTechnologyNowBinding { get; private set; } = null!;

    public void Initialize(GameInstance game, Binding<bool> windowVisibility, TooltipFactory tooltipFactory)
    {
        this.game = game;
        this.windowVisibility = windowVisibility;
        TooltipFactory = tooltipFactory;

        if (!this.game.Me.Faction.TryGetBehaviorIncludingAncestors(out factionTechnology))
        {
            throw new InvalidOperationException("Cannot show technology window when player does not have technology.");
        }
        TechTree = TechTree.FromBlueprints(game.Blueprints.Technologies, factionTechnology, game.State.Meta.Events);
        CanUnlockTechnologyNowBinding = new Binding<bool>();

        game.State.Meta.Events.Subscribe<TechnologyTokenAwarded>(this);
        game.State.Meta.Events.Subscribe<TechnologyTokenConsumed>(this);
    }

    public void Terminate()
    {
        TechTree.Dispose();
        game.State.Meta.Events.Unsubscribe<TechnologyTokenAwarded>(this);
        game.State.Meta.Events.Unsubscribe<TechnologyTokenConsumed>(this);
    }

    public void RequestTechnologyUnlock(ITechnologyBlueprint technology)
    {
        game.Request(UnlockTechnology.Request(game.Me.Faction, technology));
    }

    [Conditional("DEBUG")]
    public void ForceTechnologyUnlock(ITechnologyBlueprint technology)
    {
        game.Request(ForceUnlockTechnology.Request(game.Me.Faction, technology));
    }

    public void CloseWindow()
    {
        windowVisibility.SetFromControl(false);
    }

    public void HandleEvent(TechnologyTokenAwarded @event)
    {
        if (@event.FactionTechnology != factionTechnology) return;
        CanUnlockTechnologyNowBinding.SetFromSource(true);
    }

    public void HandleEvent(TechnologyTokenConsumed @event)
    {
        if (@event.FactionTechnology != factionTechnology) return;
        CanUnlockTechnologyNowBinding.SetFromSource(false);
    }
}
