using amulware.Graphics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    sealed class UnitBlueprint : IBlueprint
    {
        public string Name { get; }
        public int Health { get; }
        public int Damage { get; }
        public TimeSpan TimeBetweenAttacks { get; }
        public Speed Speed { get; }
        public float Value { get; }
        public Color Color { get; }

        public UnitBlueprint(
            string name,
            int health,
            int damage,
            TimeSpan timeBetweenAttacks,
            Speed speed,
            float value,
            Color color)
        {
            Name = name;
            Health = health;
            Damage = damage;
            TimeBetweenAttacks = timeBetweenAttacks;
            Speed = speed;
            Value = value;
            Color = color;
        }
    }
}
