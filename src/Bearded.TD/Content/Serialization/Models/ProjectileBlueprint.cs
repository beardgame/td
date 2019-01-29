using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Projectiles;
using Bearded.Utilities;

// ReSharper disable MemberCanBePrivate.Global

namespace Bearded.TD.Content.Serialization.Models
{
    sealed class ProjectileBlueprint : IConvertsTo<Content.Models.ProjectileBlueprint, Void>
    {
        public string Id { get; set; }
        public Color Color { get; set; }
        public ISprite Sprite { get; set; }
        public List<IComponent> Components { get; set; }

        public Content.Models.ProjectileBlueprint ToGameModel(Void _)
        {
            return new Content.Models.ProjectileBlueprint(Id, Color, Sprite,
                Components?.Select(ComponentFactories.CreateComponentFactory<Projectile>));
        }
    }
}
