using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Units;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.Simulation.StatusEffects;

/*
 * Open questions:
 * 1) Do we take into account tile visibility? (Does not right now)
 * 2) If this tower gets upgraded to have a stronger effect, do we update the modifications currently applied?
 */
[Component("statusEffectEmitter")]
sealed class StatusEffectEmitter : Component<StatusEffectEmitter.IParameters>, IListener<DrawComponents>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        AttributeType AttributeAffected { get; }

        // If true, the attribute will be multiplied by (1 - ModificationValue)
        bool IsReduction { get; }

        [Modifiable(Type = AttributeType.EffectStrength)]
        double ModificationValue { get; }

        [Modifiable(Type = AttributeType.Range)]
        Unit Range { get; }
    }

    private readonly HashSet<GameObject> affectedObjects = new();

    private IBuildingState? buildingState;
    private TileRangeDrawer? tileRangeDrawer;

    private Id<Modification> modificationId;
    private UnitLayer unitLayer = null!;
    private Tile ownerTile;
    private Unit range;
    private ImmutableArray<Tile> tilesInRange;

    public StatusEffectEmitter(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        modificationId = Owner.Game.GamePlayIds.GetNext<Modification>();
        unitLayer = Owner.Game.UnitLayer;
        ownerTile = Level.GetTile(Owner.Position.XY());
        range = Parameters.Range;
        recalculateTilesInRange();

        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, p =>
        {
            buildingState = p.State;
            tileRangeDrawer = new TileRangeDrawer(
                Owner.Game, () => buildingState.RangeDrawing, () => tilesInRange, Color.Green);
        });

        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (buildingState?.IsFunctional == false)
        {
            removeModificationsFromAllUnits();
            return;
        }

        ensureRangeUpToDate();

        removeModificationsFromUnitsOutOfRange();
        addModificationsToNewUnitsInRange();
    }

    private void removeModificationsFromAllUnits()
    {
        if (affectedObjects.Count == 0)
            return;

        var upgradeEffect = createUpgradeEffect();

        foreach (var unit in affectedObjects)
        {
            unit.RemoveUpgradeEffect(upgradeEffect);
        }
    }

    private void ensureRangeUpToDate()
    {
        if (range == Parameters.Range && Level.GetTile(Owner.Position) == ownerTile) return;
        range = Parameters.Range;
        recalculateTilesInRange();
    }

    private void removeModificationsFromUnitsOutOfRange()
    {
        var unitsOutOfRange =
            affectedObjects.Where(unit =>
                !tilesInRange.OverlapsWithTiles(OccupiedTileAccumulator.AccumulateOccupiedTiles(unit)));
        var upgradeEffect = createUpgradeEffect();

        foreach (var unit in unitsOutOfRange)
        {
            unit.RemoveUpgradeEffect(upgradeEffect);
        }
    }

    private ParameterModifiableWithId createUpgradeEffect()
    {
        var modificationFactor =
            Parameters.IsReduction ? 1 - Parameters.ModificationValue : Parameters.ModificationValue;
        return new ParameterModifiableWithId(
            Parameters.AttributeAffected,
            new ModificationWithId(modificationId, Modification.MultiplyWith(modificationFactor)),
            UpgradePrerequisites.Empty);
    }

    private void addModificationsToNewUnitsInRange()
    {
        foreach (var unit in tilesInRange.SelectMany(unitLayer.GetUnitsOnTile))
        {
            if (affectedObjects.Contains(unit)) continue;

            var upgradeEffect = createUpgradeEffect();
            if (!unit.CanApplyUpgradeEffect(upgradeEffect)) continue;

            unit.ApplyUpgradeEffect(upgradeEffect);
            affectedObjects.Add(unit);
        }
    }

    public void HandleEvent(DrawComponents e)
    {
        tileRangeDrawer?.Draw();
    }

    private void recalculateTilesInRange()
    {
        var level = Owner.Game.Level;

        var tile = Level.GetTile(Owner.Position);

        if (!level.IsValid(tile))
        {
            tilesInRange = ImmutableArray<Tile>.Empty;
            return;
        }

        var tileRadius = (int)(range.NumericValue * (1 / HexagonWidth) + HexagonWidth);
        tilesInRange = ImmutableArray.CreateRange(Tilemap.GetSpiralCenteredAt(tile, tileRadius));
    }
}
