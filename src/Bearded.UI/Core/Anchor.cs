namespace Bearded.UI
{
    public struct Anchor
    {
        public readonly double Percentage;
        public readonly double Offset;

        public Anchor(double percentage, double offset)
        {
            Percentage = percentage;
            Offset = offset;
        }
    }
}
