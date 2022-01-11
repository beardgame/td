using System;
using Bearded.TD.Game;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class ActionBar : IListener<BuildingTechnologyUnlocked>
{
    public event VoidEventHandler? ActionsChanged;

    private readonly InteractionHandler?[] handlers = new InteractionHandler[Constants.Game.GameUI.ActionBarSize];
    private readonly (string, string)[] labels = new (string, string)[Constants.Game.GameUI.ActionBarSize];
    private GameInstance game = null!;
    private int lastFilledIndex = -1;

    public void Initialize(GameInstance game)
    {
        this.game = game;

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

    private void addBuilding(IComponentOwnerBlueprint blueprint)
    {
        if (lastFilledIndex == Constants.Game.GameUI.ActionBarSize - 2)
        {
            throw new InvalidOperationException("Tried adding new building, but action bar is full D:");
        }

        lastFilledIndex++;
        handlers[lastFilledIndex] = new BuildingInteractionHandler(game, game.Me.Faction, blueprint);
        labels[lastFilledIndex] = (blueprint.GetName(), $"{blueprint.GetResourceCost().NumericValue}");
    }
}
