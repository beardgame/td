using OpenTK.Input;

namespace Bearded.UI.EventArgs
{
    public class KeyEventArgs : RoutedEventArgs
    {
        public Key Key { get; }

        public KeyEventArgs(Key key)
        {
            Key = key;
        }
    }
}
