using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Projectiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    sealed class ProjectileBlueprint : IBlueprint
    {
        public string Id { get; }
        public Speed Speed { get; }
        public float Damage { get; }
        public Color Color { get; }

        private readonly IReadOnlyList<IComponentFactory<Projectile>> componentFactories;

        public IEnumerable<IComponent<Projectile>> GetComponents()
            => componentFactories.Select(f => f.Create());

        public ProjectileBlueprint(
                string id,
                Speed speed,
                float damage,
                Color color,
                IEnumerable<IComponentFactory<Projectile>> componentFactories)
        {
            Id = id;
            Speed = speed;
            Damage = damage;
            Color = color;
            this.componentFactories =
                    (componentFactories?.ToList() ?? new List<IComponentFactory<Projectile>>())
                            .AsReadOnly();
        }
    }
}
