using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Rendering;
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

                if (!Owner.Game.BuildingLayer.TryGetMaterializedBuilding(
                    Owner.CurrentTile.Neighbor(desiredDirection), out var target))
                {
                    return;
                }
                if (!target.TryGetSingleComponent<IDamageReceiver>(out var damageReceiver))
                {
                    return;
                }

                Owner.TryGetSingleComponentInOwnerTree<IDamageSource>(out var damageSource);

                var result = damageReceiver.Damage(new DamageInfo(Parameters.Damage, DamageType.Kinetic, damageSource));
                Events.Send(new CausedDamage(target, result));
                nextAttack += 1 / Parameters.AttackRate;
            }
        }

        public override void Draw(CoreDrawers drawers) { }
    }
}
