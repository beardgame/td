using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Projectiles;

namespace Bearded.TD.Mods.Models
{
    sealed class ProjectileBlueprint : IProjectileBlueprint
    {
        public string Id { get; }
        public Color Color { get; }
        public ISprite Sprite { get; }

        private readonly IReadOnlyList<IComponentFactory<Projectile>> componentFactories;

        public IEnumerable<IComponent<Projectile>> GetComponents()
            => componentFactories.Select(f => f.Create());

        public ProjectileBlueprint(string id,
            Color color,
            ISprite sprite,
            IEnumerable<IComponentFactory<Projectile>> componentFactories)
        {
            Id = id;
            Color = color;
            Sprite = sprite;
            this.componentFactories =
                (componentFactories?.ToList() ?? new List<IComponentFactory<Projectile>>())
                    .AsReadOnly();
        }
    }
}
