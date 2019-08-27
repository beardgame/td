using Bearded.TD.Game;
using Bearded.TD.Game.Technologies;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class TechnologyUI : IListener<TechnologyQueued>, IListener<TechnologyUnlocked>
    {
        public GameInstance Game { get; private set; }

        public event VoidEventHandler TechnologiesUpdated;

        public void Initialize(GameInstance game)
        {
            Game = game;
            Game.State.Meta.Events.Subscribe<TechnologyQueued>(this);
            Game.State.Meta.Events.Subscribe<TechnologyUnlocked>(this);
        }

        public void Terminate()
        {
            Game.State.Meta.Events.Unsubscribe<TechnologyQueued>(this);
            Game.State.Meta.Events.Unsubscribe<TechnologyUnlocked>(this);
        }

        public void HandleEvent(TechnologyQueued @event)
        {
            TechnologiesUpdated?.Invoke();
        }

        public void HandleEvent(TechnologyUnlocked @event)
        {
            TechnologiesUpdated?.Invoke();
        }
    }
}
