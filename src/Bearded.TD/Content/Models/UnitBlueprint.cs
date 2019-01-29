using amulware.Graphics;
using Bearded.TD.Game.Units;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    sealed class UnitBlueprint : IUnitBlueprint
    {
        public string Id { get; }
        public string Name { get; }
        public int Health { get; }
        public int Damage { get; }
        public TimeSpan TimeBetweenAttacks { get; }
        public Speed Speed { get; }
        public float Value { get; }
        public Color Color { get; }

        public UnitBlueprint(
            string id,
            string name,
            int health,
            int damage,
            TimeSpan timeBetweenAttacks,
            Speed speed,
            float value,
            Color color)
        {
            Id = id;
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
