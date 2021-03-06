using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components.Damage
{
    [Component("splashDamageOnHit")]
    sealed class ProjectileSplashDamage : Component<Projectile, IProjectileSplashDamageComponentParameters>,
        IListener<HitLevel>, IListener<HitEnemy>
    {
        public ProjectileSplashDamage(IProjectileSplashDamageComponentParameters parameters) : base(parameters) {}

        protected override void Initialize()
        {
            Events.Subscribe<HitLevel>(this);
            Events.Subscribe<HitEnemy>(this);
        }

        public void HandleEvent(HitLevel @event)
        {
            onHit();
        }

        public void HandleEvent(HitEnemy @event)
        {
            onHit();
        }

        private void onHit()
        {
            var center = Owner.Position;
            var distanceSquared = Parameters.Range.Squared;

            var enemies = Owner.Game.UnitLayer;
            // Returns only tiles with their centre in the circle with the given range.
            // This means it may miss enemies that are strictly speaking in range, but are on a tile that itself is out
            // of range.
            var tiles = Level.TilesWithCenterInCircle(center.XY(), Parameters.Range);

            foreach (var enemy in tiles.SelectMany(enemies.GetUnitsOnTile))
            {
                if ((enemy.Position - center).LengthSquared > distanceSquared)
                {
                    continue;
                }

                var result = enemy.Damage(new DamageInfo(Parameters.Damage, DamageType.Kinetic, Owner.DamageSource));
                Events.Send(new CausedDamage(enemy, result));
            }
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers) { }
    }
}
