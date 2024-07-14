using System;
using System.Diagnostics;
using Bearded.TD.Audio;
using Bearded.TD.Content;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Model;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.TD.UI.Shortcuts;
using Bearded.TD.Utilities;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls;

sealed class TechnologyWindow : IListener<TechnologyTokenAwarded>, IListener<TechnologyTokenConsumed>
{
    private readonly ShortcutLayer alwaysActiveShortcuts;
    private readonly ShortcutLayer visibleOnlyShortcuts;

    private GameInstance game = null!;
    private ShortcutCapturer shortcutCapturer = null!;
    private FactionTechnology factionTechnology = null!;
    private Binding<bool> windowVisibility = null!;
    private ContentManager content = null!;
    public TechTree TechTree { get; private set; } = null!;
    public Binding<bool> CanUnlockTechnologyNowBinding { get; private set; } = null!;

    public TechnologyWindow()
    {
        alwaysActiveShortcuts = ShortcutLayer.CreateBuilder()
            .AddShortcut(Keys.T, toggleWindow)
            .Build();
        visibleOnlyShortcuts = ShortcutLayer.CreateBuilder()
            .AddShortcut(Keys.Escape, CloseWindow)
            .Build();
    }

    public void Initialize(
        GameInstance game,
        Binding<bool> windowVisibility,
        ShortcutCapturer shortcutCapturer,
        ContentManager content)
    {
        this.game = game;
        this.shortcutCapturer = shortcutCapturer;
        this.windowVisibility = windowVisibility;
        this.content = content;

        if (!this.game.Me.Faction.TryGetBehaviorIncludingAncestors(out factionTechnology))
        {
            throw new InvalidOperationException("Cannot show technology window when player does not have technology.");
        }
        TechTree = TechTree.FromBlueprints(game.Blueprints.Technologies, factionTechnology, game.State.Meta.Events);
        CanUnlockTechnologyNowBinding = new Binding<bool>();

        shortcutCapturer.AddLayer(alwaysActiveShortcuts);
        game.State.Meta.Events.Subscribe<TechnologyTokenAwarded>(this);
        game.State.Meta.Events.Subscribe<TechnologyTokenConsumed>(this);
        windowVisibility.SourceUpdated += syncWindowVisibility;
    }

    public void Terminate()
    {
        TechTree.Dispose();
        game.State.Meta.Events.Unsubscribe<TechnologyTokenAwarded>(this);
        game.State.Meta.Events.Unsubscribe<TechnologyTokenConsumed>(this);
        shortcutCapturer.RemoveLayer(alwaysActiveShortcuts);
        windowVisibility.SourceUpdated -= syncWindowVisibility;
    }

    public void RequestTechnologyUnlock(ITechnologyBlueprint technology)
    {
        game.Request(UnlockTechnology.Request(game.Me.Faction, technology));

        var sound = technology.Branch.ToElement().GetUpgradeSound(content);
        game.State.Meta.SoundScape.PlayGlobalSound(sound);
    }

    [Conditional("DEBUG")]
    public void ForceTechnologyUnlock(ITechnologyBlueprint technology)
    {
        game.Request(ForceUnlockTechnology.Request(game.Me.Faction, technology));
    }

    private void syncWindowVisibility(bool newVisibility)
    {
        if (newVisibility)
        {
            OpenWindow();
        }
        else
        {
            CloseWindow();
        }
    }

    private void toggleWindow()
    {
        if (windowVisibility.Value)
        {
            CloseWindow();
        }
        else
        {
            OpenWindow();
        }
    }

    public void OpenWindow()
    {
        windowVisibility.SetFromControl(true);
        shortcutCapturer.AddLayer(visibleOnlyShortcuts);
    }

    public void CloseWindow()
    {
        windowVisibility.SetFromControl(false);
        shortcutCapturer.RemoveLayer(visibleOnlyShortcuts);
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
