using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Components;

namespace Bearded.TD.Game.Projectiles
{
    interface IProjectileBlueprint : IBlueprint
    {
        int Damage { get; }
        Color Color { get; }

        IEnumerable<IComponent<Projectile>> GetComponents();
    }
}