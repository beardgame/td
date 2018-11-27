using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Units.StatusEffects;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("statusEmitter")]
    sealed class StatusEmitter<T> : Component<T, IStatusEmitterParameters>
        where T : GameObject, IPositionable, ISelectable
    {
        private readonly HashSet<EnemyUnit> affectedUnits = new HashSet<EnemyUnit>();
        private readonly List<StatusEffectSource> activeStatusEffects = new List<StatusEffectSource>();

        private readonly IUnitStatusEffect statusEffect;
        private Building ownerAsBuilding;

        private Instant nextTilesInRangeRecalculationTime;
        private Instant nextUnitsInRangeRecalculationTime;
        private List<Tile> tilesInRange;

        public StatusEmitter(IStatusEmitterParameters parameters) : base(parameters)
        {
            statusEffect = UnitStatusEffects.Slow;
        }

        protected override void Initialise()
        {
            nextTilesInRangeRecalculationTime = Owner.Game.Time;
            nextUnitsInRangeRecalculationTime = Owner.Game.Time;
            ownerAsBuilding = Owner as Building;

            Owner.Deleting += onOwnerDeleted;
        }

        private void onOwnerDeleted()
        {
            foreach (var effect in activeStatusEffects)
            {
                effect.EndImmediately();
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (!ownerAsBuilding?.IsCompleted ?? false) return;

            updateActiveStatusEffects();

            if (nextUnitsInRangeRecalculationTime > Owner.Game.Time) return;

            ensureTilesInRangeUpToDate();

            var newUnitsInRange = tilesInRange
                .SelectMany(Owner.Game.UnitLayer.GetUnitsOnTile)
                .Where(enemy => !affectedUnits.Contains(enemy));
            foreach (var unit in newUnitsInRange)
            {
                var statusEffectForUnit = new StatusEffectSource(unit, statusEffect, isTileInRange);
                unit.ApplyStatusEffect(statusEffectForUnit);
                activeStatusEffects.Add(statusEffectForUnit);
                affectedUnits.Add(unit);
            }

            nextUnitsInRangeRecalculationTime = Owner.Game.Time + Parameters.RecalculateUnitsInRangeInterval;
        }

        private void updateActiveStatusEffects()
        {
            foreach (var effect in activeStatusEffects.Where(effect => effect.HasEnded))
            {
                affectedUnits.Remove(effect.Unit);
            }
            activeStatusEffects.RemoveAll(effect => effect.HasEnded);
        }

        private void ensureTilesInRangeUpToDate()
        {
            if (Owner.Game.Time >= nextTilesInRangeRecalculationTime)
            {
                recalculateTilesInRange();
            }
        }

        private void recalculateTilesInRange()
        {
            var rangeSquared = Parameters.Range.Squared;

            tilesInRange = new LevelVisibilityChecker()
                    .EnumerateVisibleTiles(Owner.Game.Level, Owner.Position, Parameters.Range,
                            t => !t.IsValid || !t.Info.IsPassableFor(TileInfo.PassabilityLayer.Projectile))
                    .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                            (Owner.Game.Level.GetPosition(t.tile) - Owner.Position).LengthSquared < rangeSquared)
                    .Select(t => t.tile)
                    .ToList();

            nextTilesInRangeRecalculationTime = Owner.Game.Time + Parameters.RecalculateTilesInRangeInterval;
        }

        public override void Draw(GeometryManager geometries)
        {
            if (ownerAsBuilding == null || ownerAsBuilding.SelectionState == SelectionState.Default) return;

            recalculateTilesInRange();

            var geo = geometries.ConsoleBackground;

            geo.Color = Color.Green * (ownerAsBuilding.SelectionState == SelectionState.Selected ? 0.15f : 0.1f);

            var level = ownerAsBuilding.Game.Level;

            foreach (var tile in tilesInRange)
            {
                geo.DrawCircle(level.GetPosition(tile).NumericValue, Constants.Game.World.HexagonSide, true, 6);
            }
        }

        private bool isTileInRange(Tile tile)
        {
            return tilesInRange.Contains(tile);
        }

        private class StatusEffectSource : IStatusEffectSource
        {
            public EnemyUnit Unit { get; }
            public IUnitStatusEffect Effect { get; }

            private readonly Func<Tile, bool> rangeChecker;

            public bool HasEnded { get; private set; }

            public StatusEffectSource(EnemyUnit unit, IUnitStatusEffect effect, Func<Tile, bool> rangeChecker)
            {
                Unit = unit;
                Effect = effect;
                this.rangeChecker = rangeChecker;
            }

            public void Update(TimeSpan elapsedTime)
            {
                HasEnded = !rangeChecker(Unit.CurrentTile);
            }

            public void EndImmediately()
            {
                HasEnded = true;
            }
        }
    }
}
