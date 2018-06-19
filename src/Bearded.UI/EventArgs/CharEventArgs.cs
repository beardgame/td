namespace Bearded.UI.EventArgs
{
    public class CharEventArgs : RoutedEventArgs
    {
        public char Character { get; }

        public CharEventArgs(char character)
        {
            Character = character;
        }
    }
}
