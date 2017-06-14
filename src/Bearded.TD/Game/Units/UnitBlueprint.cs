using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Units
{
    class UnitBlueprint : IIdable<UnitBlueprint>, INamed
    {
        public Id<UnitBlueprint> Id { get; }
        public string Name { get; }
        public int Health { get; }
        public int Damage { get; }
        public Speed Speed { get; }
        public float Value { get; }

        public UnitBlueprint(Id<UnitBlueprint> id, string name, int health, int damage, Speed speed, float value)
        {
            Id = id;
            Name = name;
            Health = health;
            Damage = damage;
            Speed = speed;
            Value = value;
        }
    }
}
