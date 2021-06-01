using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class TechnologyUI
        : IListener<TechnologyDequeued>, IListener<TechnologyQueued>, IListener<TechnologyUnlocked>
    {
        public GameInstance Game { get; private set; } = null!;
        public TechnologyUIModel Model { get; private set; } = null!;
        private FactionTechnology? technology;

        public event VoidEventHandler? TechnologiesUpdated;

        public void Initialize(GameInstance game)
        {
            Game = game;
            Model = new TechnologyUIModel(game);
            Game.Me.Faction.TryGetBehaviorIncludingAncestors(out technology);
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
            if (technology == null || @event.FactionTechnology != technology)
            {
                return;
            }
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }

        public void HandleEvent(TechnologyQueued @event)
        {
            if (technology == null || @event.FactionTechnology != technology)
            {
                return;
            }
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }

        public void HandleEvent(TechnologyUnlocked @event)
        {
            if (technology == null || @event.FactionTechnology != technology)
            {
                return;
            }
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }
    }
}
