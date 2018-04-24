using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("projectileEmitter")]
    sealed class ProjectileEmitter : IComponent<Weapon>
    {
        private readonly ProjectileEmitterParameters parameters;
        public Weapon Weapon { get; private set; }

        private Instant nextPossibleShootTime;
        private bool wasShootingLastFrame;

        private GameState game => Weapon.Owner.Game;
        Weapon IComponent<Weapon>.Owner => Weapon;

        public ProjectileEmitter(ProjectileEmitterParameters parameters)
        {
            this.parameters = parameters;
        }
        
        public void OnAdded(Weapon owner)
        {
            Weapon = owner;
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (Weapon.ShootingThisFrame)
            {
                updateShooting();
            }
            else
            {
                updateIdle();
            }
        }

        private void updateIdle()
        {
            wasShootingLastFrame = false;
        }

        private void updateShooting()
        {
            var currentTime = game.Time;
            while (nextPossibleShootTime < currentTime)
            {
                emitProjectile();

                if (!wasShootingLastFrame)
                {
                    nextPossibleShootTime = currentTime + parameters.ShootInterval;
                    break;
                }

                nextPossibleShootTime += parameters.ShootInterval;
            }
            wasShootingLastFrame = true;
        }

        private void emitProjectile()
        {
            game.Add(
                new Projectile(
                    parameters.Projectile,
                    Weapon.Position, Weapon.AimDirection,
                    parameters.MuzzleVelocity,
                    Weapon.Owner as Building
                )
            );
        }
        
        public void Draw(GeometryManager geometries)
        {
        }
    }
}
