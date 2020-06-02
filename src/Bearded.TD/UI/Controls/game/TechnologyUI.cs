using Bearded.TD.Game;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Technologies;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class TechnologyUI
        : IListener<TechnologyDequeued>, IListener<TechnologyQueued>, IListener<TechnologyUnlocked>
    {
        public GameInstance Game { get; private set; }
        public TechnologyUIModel Model { get; private set; }

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
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }

        public void HandleEvent(TechnologyQueued @event)
        {
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }

        public void HandleEvent(TechnologyUnlocked @event)
        {
            Model.UpdateTechnology(@event.Technology);
            TechnologiesUpdated?.Invoke();
        }
    }
}
