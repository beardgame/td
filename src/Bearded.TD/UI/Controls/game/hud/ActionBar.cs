using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.TD.UI.Shortcuts;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static Bearded.TD.Constants.Game.GameUI;

namespace Bearded.TD.UI.Controls;

sealed class ActionBar : IListener<BuildingTechnologyUnlocked>
{
    private static readonly ImmutableArray<Keys> numberKeys =
        ImmutableArray.Create(Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0);

    public Binding<ImmutableArray<ActionBarEntry?>> Entries { get; } = new();

    private readonly ShortcutLayer shortcuts;

    private GameInstance game = null!;
    private ShortcutCapturer shortcutCapturer = null!;
    private ContentManager contentManager = null!;
    private GridVisibility gridVisibility = null!;

    public ActionBar()
    {
        shortcuts = createShortcuts();
    }

    private ShortcutLayer createShortcuts()
    {
        var builder = ShortcutLayer.CreateBuilder();
        for (var i = 0; i < Math.Min(numberKeys.Length, ActionBarSize); i++)
        {
            var idx = i;
            builder.AddShortcut(numberKeys[i], () => Entries.Value[idx]?.OnClick());
        }
        return builder.Build();
    }

    // ReSharper disable ParameterHidesMember
    public void Initialize(
        GameInstance game,
        ShortcutCapturer shortcutCapturer,
        ContentManager contentManager,
        GridVisibility gridVisibility)
    {
        this.game = game;
        this.shortcutCapturer = shortcutCapturer;
        this.contentManager = contentManager;
        this.gridVisibility = gridVisibility;
        shortcutCapturer.AddLayer(shortcuts);

        var buildingEntries = game.Me.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology)
            ? technology.UnlockedBuildings.Select(makeEntryFromBlueprint).AsNullable().ToImmutableArray()
            : ImmutableArray<ActionBarEntry?>.Empty;
        var missingEntryCount = ActionBarSize - buildingEntries.Length;
        var actionBarEntries = missingEntryCount <= 0
            ? buildingEntries
            : buildingEntries.Concat(Enumerable.Repeat((ActionBarEntry?) null, missingEntryCount)).ToImmutableArray();
        Entries.SetFromSource(actionBarEntries);

        game.Meta.Events.Subscribe(this);
    }

    public void Terminate()
    {
        shortcutCapturer.RemoveLayer(shortcuts);
        game.Meta.Events.Unsubscribe(this);
    }

    public void HandleEvent(BuildingTechnologyUnlocked @event)
    {
        if (!game.Me.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var myTechnology)
            || myTechnology != @event.FactionTechnology)
        {
            return;
        }

        addBuilding(@event.Blueprint);
    }

    private void addBuilding(IGameObjectBlueprint blueprint)
    {
        var existingEntries = Entries.Value;
        var firstNonEmptyIndex = existingEntries.IndexOf(null);

        var newEntry = makeEntryFromBlueprint(blueprint);
        var newEntries = firstNonEmptyIndex >= 0
            ? existingEntries.SetItem(firstNonEmptyIndex, newEntry)
            : existingEntries.Add(newEntry);

        Entries.SetFromSource(newEntries);
    }

    private ActionBarEntry makeEntryFromBlueprint(IGameObjectBlueprint blueprint)
    {
        var handler = new BuildingInteractionHandler(game, game.Me.Faction, blueprint, contentManager, gridVisibility);
        var attributes = blueprint.AttributesOrDefault();
        return new ActionBarEntry(
            attributes.Name,
            blueprint.GetResourceCost(),
            attributes.Icon ?? Constants.Content.CoreUI.Sprites.CheckMark,
            onClick);

        void onClick() => game.PlayerInput.SetInteractionHandler(handler);
    }
}

sealed record ActionBarEntry(string Label, Resource<Scrap> Cost, ModAwareSpriteId Icon, VoidEventHandler OnClick);
