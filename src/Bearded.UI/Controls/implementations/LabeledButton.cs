namespace Bearded.UI.Controls
{
    public class LabeledButton<T> : Button
    {
        public T Label { get; set; }

        public LabeledButton(T label)
        {
            Label = label;
        }
    }
}
