using System;
using System.Collections.Immutable;
using Bearded.TD.Game;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.TD.UI.Shortcuts;
using Bearded.Utilities;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Controls;

sealed class ActionBar : IListener<BuildingTechnologyUnlocked>
{
    private static ImmutableArray<Keys> numberKeys =
        ImmutableArray.Create(Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0);

    public event VoidEventHandler? ActionsChanged;

    private readonly InteractionHandler?[] handlers = new InteractionHandler[Constants.Game.GameUI.ActionBarSize];
    private readonly (string, string)[] labels = new (string, string)[Constants.Game.GameUI.ActionBarSize];

    private readonly ShortcutLayer shortcuts;

    private GameInstance game = null!;
    private ShortcutCapturer shortcutCapturer = null!;
    private int lastFilledIndex = -1;

    public ActionBar()
    {
        shortcuts = createShortcuts();
    }

    private ShortcutLayer createShortcuts()
    {
        var builder = ShortcutLayer.CreateBuilder();
        for (var i = 0; i < Math.Min(numberKeys.Length, Constants.Game.GameUI.ActionBarSize); i++)
        {
            var idx = i;
            builder.AddShortcut(numberKeys[i], () => OnActionClicked(idx));
        }
        return builder.Build();
    }

    public void Initialize(GameInstance game, ShortcutCapturer shortcutCapturer)
    {
        this.game = game;
        this.shortcutCapturer = shortcutCapturer;
        shortcutCapturer.AddLayer(shortcuts);

        if (game.Me.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology))
        {
            foreach (var b in technology.UnlockedBuildings)
            {
                addBuilding(b);
            }
        }

        game.Meta.Events.Subscribe(this);
        ActionsChanged?.Invoke();
    }

    public void Terminate()
    {
        shortcutCapturer.RemoveLayer(shortcuts);
        game.Meta.Events.Unsubscribe(this);
    }

    public (string actionLabel, string cost) ActionLabelForIndex(int i) => labels[i];

    public void OnActionClicked(int actionIndex)
    {
        if (handlers[actionIndex] == null) return;
        game.PlayerInput.SetInteractionHandler(handlers[actionIndex]);
    }

    public void HandleEvent(BuildingTechnologyUnlocked @event)
    {
        if (!game.Me.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var myTechnology)
            || myTechnology != @event.FactionTechnology)
        {
            return;
        }

        addBuilding(@event.Blueprint);
        ActionsChanged?.Invoke();
    }

    private void addBuilding(IGameObjectBlueprint blueprint)
    {
        if (lastFilledIndex == Constants.Game.GameUI.ActionBarSize - 2)
        {
            throw new InvalidOperationException("Tried adding new building, but action bar is full D:");
        }

        lastFilledIndex++;
        handlers[lastFilledIndex] = new BuildingInteractionHandler(game, game.Me.Faction, blueprint);
        labels[lastFilledIndex] = (blueprint.NameOrDefault(), $"{blueprint.GetResourceCost().NumericValue}");
    }
}
