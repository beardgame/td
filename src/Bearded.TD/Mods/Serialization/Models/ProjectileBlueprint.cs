using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Projectiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class ProjectileBlueprint : IConvertsTo<Mods.Models.ProjectileBlueprint, Void>
    {
        public string Id { get; set; }
        public Speed Speed { get; set; }
        public float Damage { get; set; }
        public Color Color { get; set; }
        public List<IComponent> Components { get; set; }

        public Mods.Models.ProjectileBlueprint ToGameModel(Void _)
        {
            return new Mods.Models.ProjectileBlueprint(Id, Speed, Damage, Color,
                    Components?.Select(ComponentFactories.CreateComponentFactory<Projectile>));
        }
    }
}
