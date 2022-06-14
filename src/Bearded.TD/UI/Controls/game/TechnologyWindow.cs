using System;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class TechnologyWindow
{
    private GameInstance game = null!;
    private Binding<bool> windowVisibility = null!;

    public TooltipFactory TooltipFactory { get; private set; } = null!;
    public TechTree TechTree { get; private set; } = null!;

    public void Initialize(GameInstance game, Binding<bool> windowVisibility, TooltipFactory tooltipFactory)
    {
        this.game = game;
        this.windowVisibility = windowVisibility;
        TooltipFactory = tooltipFactory;

        if (!this.game.Me.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var factionTechnology))
        {
            throw new InvalidOperationException("Cannot show technology window when player does not have technology.");
        }
        TechTree = TechTree.FromBlueprints(game.Blueprints.Technologies, factionTechnology, game.State.Meta.Events);
    }

    public void Terminate()
    {
        TechTree.Dispose();
    }

    public void RequestTechnologyUnlock(ITechnologyBlueprint technology)
    {
        game.Request(UnlockTechnology.Request(game.Me.Faction, technology));
    }

    public void CloseWindow()
    {
        windowVisibility.SetFromControl(false);
    }
}
