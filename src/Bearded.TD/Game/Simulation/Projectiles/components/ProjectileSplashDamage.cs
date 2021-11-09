using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    [Component("splashDamageOnHit")]
    sealed class ProjectileSplashDamage : Component<ComponentGameObject, IProjectileSplashDamageComponentParameters>,
        IListener<HitLevel>, IListener<HitEnemy>
    {
        public ProjectileSplashDamage(IProjectileSplashDamageComponentParameters parameters) : base(parameters) {}

        protected override void OnAdded()
        {
            Events.Subscribe<HitLevel>(this);
            Events.Subscribe<HitEnemy>(this);
        }

        public override void OnRemoved()
        {
            Events.Unsubscribe<HitLevel>(this);
            Events.Unsubscribe<HitEnemy>(this);
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

            Owner.TryGetSingleComponentInOwnerTree<IDamageSource>(out var damageSource);

            foreach (var enemy in tiles.SelectMany(enemies.GetUnitsOnTile))
            {
                if ((enemy.Position - center).LengthSquared > distanceSquared
                    || !enemy.TryGetSingleComponent<IDamageReceiver>(out var damageReceiver))
                {
                    continue;
                }

                var result = damageReceiver
                    .Damage(new DamageInfo(Parameters.Damage, DamageType.Kinetic), damageSource);
                Events.Send(new CausedDamage(result));
            }
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers) { }
    }
}
