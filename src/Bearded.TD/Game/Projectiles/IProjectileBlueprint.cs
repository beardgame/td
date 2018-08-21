using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game.Projectiles
{
    interface IProjectileBlueprint : IBlueprint
    {
        int Damage { get; }
        Color Color { get; }
        ISprite Sprite { get; }

        IEnumerable<IComponent<Projectile>> GetComponents();
    }
}