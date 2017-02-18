using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units
{
    struct UnitBlueprint
    {
        public int Health { get; }
        public Speed Speed { get; }

        public UnitBlueprint(int health, Speed speed)
        {
            Health = health;
            Speed = speed;
        }
    }
}
