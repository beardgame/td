using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Simulation.Damage
{
    [Component("damageOverTimeArea")]
    sealed class DamageOverTimeArea<T> : Component<T, IDamageOverTimeAreaParameters>
        where T : GameObject, IPositionable, IComponentOwner
    {
        private IDamageSource? damageSource;

        public DamageOverTimeArea(IDamageOverTimeAreaParameters parameters) : base(parameters) {}

        protected override void Initialize()
        {
            damageSource = Owner.FindInComponentOwnerTree<IDamageSource>().ValueOrDefault(() => null);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var damageThisFrame = StaticRandom.Discretise(
                (float)(Parameters.DamagePerSecond * elapsedTime.NumericValue)
                ).HitPoints();

            damageAllEnemiesInArea(damageThisFrame);
        }

        private void damageAllEnemiesInArea(HitPoints damage)
        {
            var level = Owner.Game.Level;
            var centerTile = Level.GetTile(Owner.Position);

            if (!level.IsValid(centerTile))
                return;

            var tileRadius = (int)(Parameters.Range.NumericValue * (1 / HexagonWidth) + HexagonWidth);
            var units = Owner.Game.UnitLayer;

            var centerPosition = Owner.Position;
            var rangeSquared = Parameters.Range.Squared;
            var damageInfo = new DamageInfo(damage, Parameters.Type, damageSource);

            foreach (var tile in Tilemap.GetSpiralCenteredAt(centerTile, tileRadius))
            {
                foreach (var unit in units.GetUnitsOnTile(tile))
                {
                    var distanceSquared = (centerPosition - unit.Position).LengthSquared;

                    if (distanceSquared < rangeSquared
                        && unit.TryGetSingleComponent<IDamageReceiver>(out var damageReceiver))
                    {
                        var result = damageReceiver.Damage(damageInfo);
                        Events.Send(new CausedDamage(unit, result));
                    }
                }
            }
        }

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}
