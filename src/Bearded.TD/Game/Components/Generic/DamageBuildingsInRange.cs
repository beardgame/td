using Bearded.TD.Content.Models;
using Bearded.TD.Game.Units;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("damageBuildings")]
    sealed class DamageBuildingsInRange : Component<EnemyUnit, IDamageBuildingsInRangeParameters>
    {
        private Instant nextAttack;
        
        public DamageBuildingsInRange(IDamageBuildingsInRangeParameters parameters) : base(parameters) { }
        
        protected override void Initialise()
        {
            nextAttack = Owner.Game.Time + 1 / Parameters.AttackRate;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Owner.IsMoving) return;

            while (nextAttack <= Owner.Game.Time)
            {
                // TODO: add range
                var desiredDirection = Owner.Game.Navigator.GetDirections(Owner.CurrentTile);

                if (!Owner.Game.BuildingLayer.TryGetMaterializedBuilding(
                    Owner.CurrentTile.Neighbour(desiredDirection), out var target))
                {
                    return;
                }
                
                target.Damage(Parameters.Damage);
                nextAttack = Owner.Game.Time + 1 / Parameters.AttackRate;
            }
        }

        public override void Draw(GeometryManager geometries) { }
    }
}
