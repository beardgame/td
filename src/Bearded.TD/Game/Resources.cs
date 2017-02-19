using Bearded.Utilities.Math;

namespace Bearded.TD.Game
{
    class Resources
    {
        private double beardedPoints;

        public long BeardedPoints => (long)beardedPoints;

        public Resources()
        { }

        public void AddBeardedPoints(double amount)
        {
            beardedPoints += amount;
        }

        public void TakeBeardedPoints(double amount)
        {
            beardedPoints -= amount;
        }
    }
}
