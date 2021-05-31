using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class TechnologyUI
        : IListener<TechnologyDequeued>, IListener<TechnologyQueued>, IListener<TechnologyUnlocked>
    {
        public GameInstance Game { get; private set; } = null!;
        public TechnologyUIModel Model { get; private set; } = null!;

        public event VoidEventHandler? TechnologiesUpdated;

        public void Initialize(GameInstance game)
        {
            Game = game;
            Model = new TechnologyUIModel(game);
            Game.State.Meta.Events.Subscribe<TechnologyDequeued>(this);
            Game.State.Meta.Events.Subscribe<TechnologyQueued>(this);
            Game.State.Meta.Events.Subscribe<TechnologyUnlocked>(this);
        }

        public void Terminate()
        {
            Game.State.Meta.Events.Unsubscribe<TechnologyDequeued>(this);
            Game.State.Meta.Events.Unsubscribe<TechnologyQueued>(this);
            Game.State.Meta.Events.Unsubscribe<TechnologyUnlocked>(this);
        }

        public void Update()
        {
            Model.Update();
        }

        public void HandleEvent(TechnologyDequeued @event)
        {
            if (!Game.Me.Faction.SharesTechnologyWith(@event.Faction))
            {
                return;
            }
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }

        public void HandleEvent(TechnologyQueued @event)
        {
            if (!Game.Me.Faction.SharesTechnologyWith(@event.Faction))
            {
                return;
            }
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }

        public void HandleEvent(TechnologyUnlocked @event)
        {
            if (!Game.Me.Faction.SharesTechnologyWith(@event.Faction))
            {
                return;
            }
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }
    }
}
