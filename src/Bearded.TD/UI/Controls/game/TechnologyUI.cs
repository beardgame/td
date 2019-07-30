using Bearded.TD.Game;
using Bearded.TD.Game.Technologies;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class TechnologyUI : IListener<TechnologyUnlocked>
    {
        public GameInstance Game { get; private set; }

        public event VoidEventHandler TechnologiesUpdated;

        public void Initialize(GameInstance game)
        {
            Game = game;
            Game.State.Meta.Events.Subscribe(this);
        }

        public void Terminate()
        {
            Game.State.Meta.Events.Unsubscribe(this);
        }

        public void HandleEvent(TechnologyUnlocked @event)
        {
            TechnologiesUpdated?.Invoke();
        }
    }
}
