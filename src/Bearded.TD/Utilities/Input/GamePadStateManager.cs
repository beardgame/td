using OpenTK.Input;

namespace Bearded.TD.Utilities.Input
{
    class GamePadStateManager
    {
        public int Id { get; }

        public AsyncAtomicUpdating<GamePadState> State { get; } = new AsyncAtomicUpdating<GamePadState>();

        public static GamePadStateManager ForId(int id) => new GamePadStateManager(id);

        private GamePadStateManager(int id)
        {
            Id = id;
        }

        public void ProcessEventsAsync() => State.SetLastKnownState(GamePad.GetState(this.Id));

        public void Update(bool windowIsActive)
        {
            if (windowIsActive)
            {
                State.Update();
            }
            else
            {
                State.UpdateTo(default(GamePadState));
            }
        }
    }
}