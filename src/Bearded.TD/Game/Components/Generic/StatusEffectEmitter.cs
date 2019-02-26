using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Components.Generic
{
    /*
     * Open questions:
     * 1) Do we take into account tile visibility? (Does not right now)
     * 2) If this tower gets upgraded to have a stronger effect, do we update the modifications currently applied?
     */
    
    [Component("statusEffectEmitter")]
    sealed class StatusEffectEmitter<T> : Component<T, IStatusEffectEmitterParameters>
        where T : GameObject, IPositionable
    {
        // TODO: allow this to affect other things than enemies as well
        private readonly HashSet<EnemyUnit> affectedUnits = new HashSet<EnemyUnit>();
        
        private Id<Modification> modificationId;
        private UnitLayer unitLayer;
        private Unit range;
        private ImmutableList<Tile> tilesInRange;
        
        public StatusEffectEmitter(IStatusEffectEmitterParameters parameters) : base(parameters) { }
        
        protected override void Initialise()
        {
            modificationId = Owner.Game.GamePlayIds.GetNext<Modification>();
            unitLayer = Owner.Game.UnitLayer;
            range = Parameters.Range;
            recalculateTilesInRange();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            ensureRangeUpToDate();
            removeModificationsFromUnitsOutOfRange();
            addModificationsToNewUnitsInRange();
        }

        private void ensureRangeUpToDate()
        {
            if (range == Parameters.Range) return;
            range = Parameters.Range;
            recalculateTilesInRange();
        }

        private void removeModificationsFromUnitsOutOfRange()
        {
            var unitsOutOfRange = affectedUnits.Where(unit => !tilesInRange.Contains(unit.CurrentTile));
            var upgradeEffect = createUpgradeEffect();
            
            foreach (var unit in unitsOutOfRange)
            {
                unit.RemoveEffect(upgradeEffect);
            }
        }

        private ParameterModifiableWithId createUpgradeEffect()
        {
            return new ParameterModifiableWithId(
                Parameters.AttributeAffected,
                new ModificationWithId(modificationId, Modification.MultiplyWith(Parameters.ModificationValue)));
        }

        private void addModificationsToNewUnitsInRange()
        {
            foreach (var unit in tilesInRange.SelectMany(unitLayer.GetUnitsOnTile))
            {
                if (affectedUnits.Contains(unit)) continue;
                
                var upgradeEffect = createUpgradeEffect();
                if (!unit.CanApplyEffect(upgradeEffect)) continue;
                
                unit.ApplyEffect(upgradeEffect);
                affectedUnits.Add(unit);
            }
        }

        public override void Draw(GeometryManager geometries) { }

        private void recalculateTilesInRange()
        {
            var level = Owner.Game.Level;
            
            var tile = level.GetTile(Owner.Position);
            
            if (!level.IsValid(tile))
            {
                tilesInRange = ImmutableList<Tile>.Empty;
                return;
            }

            var tileRadius = (int)(range.NumericValue * (1 / HexagonWidth) + HexagonWidth);
            tilesInRange = ImmutableList.CreateRange(Tilemap.GetSpiralCenteredAt(tile, tileRadius));
        }
    }
}
