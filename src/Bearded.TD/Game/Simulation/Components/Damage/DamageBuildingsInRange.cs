using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Damage
{
    [Component("damageBuildings")]
    sealed class DamageBuildingsInRange : Component<EnemyUnit, IDamageBuildingsInRangeParameters>
    {
        private Instant nextAttack;

        public DamageBuildingsInRange(IDamageBuildingsInRangeParameters parameters) : base(parameters) { }

        protected override void Initialise()
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
                    Owner.CurrentTile.Neighbour(desiredDirection), out var target))
                {
                    return;
                }

                target.Damage(new DamageInfo(Parameters.Damage, DamageType.Kinetic, Owner));
                nextAttack += 1 / Parameters.AttackRate;
            }
        }

        public override void Draw(CoreDrawers drawers) { }
    }
}
