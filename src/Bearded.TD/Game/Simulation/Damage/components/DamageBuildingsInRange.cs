using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Units;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage
{
    [Component("damageBuildings")]
    sealed class DamageBuildingsInRange : Component<EnemyUnit, IDamageBuildingsInRangeParameters>
    {
        private Instant nextAttack;

        public DamageBuildingsInRange(IDamageBuildingsInRangeParameters parameters) : base(parameters) { }

        protected override void OnAdded()
        {
            resetAttackTime();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Owner.IsMoving)
            {
                resetAttackTime();
            }
            else
            {
                tryAttack();
            }
        }

        private void resetAttackTime()
        {
            nextAttack = Owner.Game.Time + 1 / Parameters.AttackRate;
        }

        private void tryAttack()
        {
            while (nextAttack <= Owner.Game.Time)
            {
                var desiredDirection = Owner.Game.Navigator.GetDirections(Owner.CurrentTile);

                var neighbor = Owner.CurrentTile.Neighbor(desiredDirection);
                if (!Owner.Game.BuildingLayer.TryGetMaterializedBuilding(neighbor, out var target))
                {
                    return;
                }

                var damage = new DamageInfo(Parameters.Damage, DamageType.Kinetic);
                if (DamageExecutor.FromObject(Owner).TryDoDamage(target, damage))
                {
                    nextAttack += 1 / Parameters.AttackRate;
                }
            }
        }
    }
}
