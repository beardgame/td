using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Game.Input
{
    interface ICameraController
    {
        void HandleInput(InputState input);
        void Stop();
    }
}
