using Bearded.UI.EventArgs;

namespace Bearded.UI.Events
{
    public interface IKeyboardEventsCapturer
    {
        void KeyHit(KeyEventArgs eventArgs);
        void KeyReleased(KeyEventArgs eventArgs);
    }
}
