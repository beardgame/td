using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Game.Projectiles
{
    interface IProjectileBlueprint : IBlueprint
    {
        Color Color { get; }
        ISprite Sprite { get; }

        IEnumerable<IComponent<Projectile>> GetComponents();
    }
}