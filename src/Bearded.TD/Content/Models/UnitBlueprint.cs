using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Units;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    sealed class UnitBlueprint : IUnitBlueprint
    {
        public ModAwareId Id { get; }
        public string Name { get; }
        public int Health { get; }
        public int Damage { get; }
        public TimeSpan TimeBetweenAttacks { get; }
        public Speed Speed { get; }
        public float Value { get; }
        public Color Color { get; }

        private readonly IReadOnlyList<IComponentFactory<EnemyUnit>> componentFactories;

        public IEnumerable<IComponent<EnemyUnit>> GetComponents()
            => componentFactories.Select(f => f.Create());

        public UnitBlueprint(
            ModAwareId id,
            string name,
            int health,
            int damage,
            TimeSpan timeBetweenAttacks,
            Speed speed,
            float value,
            Color color,
            IEnumerable<IComponentFactory<EnemyUnit>>? componentFactories)
        {
            Id = id;
            Name = name;
            Health = health;
            Damage = damage;
            TimeBetweenAttacks = timeBetweenAttacks;
            Speed = speed;
            Value = value;
            Color = color;

            this.componentFactories = (componentFactories?.ToList() ?? new List<IComponentFactory<EnemyUnit>>())
                .AsReadOnly();
        }
    }
}
