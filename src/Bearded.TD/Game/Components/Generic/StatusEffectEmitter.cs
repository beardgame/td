using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components.utilities;
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

        private TileRangeDrawer tileRangeDrawer;

        private Id<Modification> modificationId;
        private UnitLayer unitLayer;
        private Tile ownerTile;
        private Unit range;
        private Building ownerAsBuilding;
        private ImmutableList<Tile> tilesInRange;

        public StatusEffectEmitter(IStatusEffectEmitterParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            modificationId = Owner.Game.GamePlayIds.GetNext<Modification>();
            unitLayer = Owner.Game.UnitLayer;
            ownerTile = Level.GetTile(Owner.Position);
            range = Parameters.Range;
            ownerAsBuilding = Owner as Building;
            recalculateTilesInRange();

            if (Owner is ISelectable selectable)
            {
                tileRangeDrawer = new TileRangeDrawer(Owner.Game, selectable, () => tilesInRange);
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            ensureRangeUpToDate();

            // Don't apply status effects for uncompleted buildings
            // We probably should have a better pattern for this...
            if (!(ownerAsBuilding?.IsCompleted ?? true)) return;

            removeModificationsFromUnitsOutOfRange();
            addModificationsToNewUnitsInRange();
        }

        private void ensureRangeUpToDate()
        {
            var level = Owner.Game.Level;

            if (range == Parameters.Range && Level.GetTile(Owner.Position) == ownerTile) return;
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
            var modificationFactor =
                Parameters.IsReduction ? 1 - Parameters.ModificationValue : Parameters.ModificationValue;
            return new ParameterModifiableWithId(
                Parameters.AttributeAffected,
                new ModificationWithId(modificationId, Modification.MultiplyWith(modificationFactor)));
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

        public override void Draw(GeometryManager geometries)
        {
            tileRangeDrawer.Draw(geometries);
        }

        private void recalculateTilesInRange()
        {
            var level = Owner.Game.Level;

            var tile = Level.GetTile(Owner.Position);

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
