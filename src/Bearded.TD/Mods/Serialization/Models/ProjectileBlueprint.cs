using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Mods.Models;
using Bearded.Utilities;
// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class ProjectileBlueprint : IConvertsTo<Mods.Models.ProjectileBlueprint, Void>
    {
        public string Id { get; set; }
        public Color Color { get; set; }
        public ISprite Sprite { get; set; }
        public List<IComponent> Components { get; set; }

        public Mods.Models.ProjectileBlueprint ToGameModel(Void _)
        {
            return new Mods.Models.ProjectileBlueprint(Id, Color, Sprite,
                Components?.Select(ComponentFactories.CreateComponentFactory<Projectile>));
        }
    }
}
