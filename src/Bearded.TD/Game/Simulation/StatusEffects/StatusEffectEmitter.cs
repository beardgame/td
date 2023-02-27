using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

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

    private readonly Dictionary<GameObject, IUpgradeReceipt> affectedObjects = new();

    private IBuildingState? buildingState;
    private TileRangeDrawer? tileRangeDrawer;

    private Id<Modification> modificationId;
    private ObjectLayer objectLayer = null!;
    private Tile ownerTile;
    private Unit range;
    private ImmutableArray<Tile> tilesInRange;

    public StatusEffectEmitter(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {

        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, p =>
        {
            buildingState = p.State;
        });

        Events.Subscribe(this);
    }

    public override void Activate()
    {
        base.Activate();

        modificationId = Owner.Game.GamePlayIds.GetNext<Modification>();
        objectLayer = Owner.Game.PhysicsLayer;
        ownerTile = Level.GetTile(Owner.Position.XY());
        range = Parameters.Range;
        recalculateTilesInRange();

        tileRangeDrawer = new TileRangeDrawer(
            Owner.Game,
            () => buildingState?.RangeDrawing ?? TileRangeDrawer.RangeDrawStyle.DoNotDraw,
            () => tilesInRange,
            Color.Green);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (buildingState?.IsFunctional == false)
        {
            removeModificationsFromAllObjects();
            return;
        }

        ensureRangeUpToDate();

        removeModificationsFromObjectsOutOfRange();
        addModificationsToNewObjectsInRange();
    }

    private void removeModificationsFromAllObjects()
    {
        if (affectedObjects.Count == 0)
            return;

        foreach (var (_, upgradeReceipt) in affectedObjects)
        {
            upgradeReceipt.Rollback();
        }
    }

    private void ensureRangeUpToDate()
    {
        if (range == Parameters.Range && Level.GetTile(Owner.Position) == ownerTile) return;
        range = Parameters.Range;
        recalculateTilesInRange();
    }

    private void removeModificationsFromObjectsOutOfRange()
    {
        var effectsOutOfRange =
            affectedObjects.Where(kvp =>
                !tilesInRange.OverlapsWithTiles(OccupiedTileAccumulator.AccumulateOccupiedTiles(kvp.Key)));

        foreach (var (obj, upgradeReceipt) in effectsOutOfRange)
        {
            affectedObjects.Remove(obj);
            upgradeReceipt.Rollback();
        }
    }

    private void addModificationsToNewObjectsInRange()
    {
        var upgrade = createUpgrade();
        foreach (var obj in tilesInRange.SelectMany(objectLayer.GetObjectsOnTile))
        {
            if (affectedObjects.ContainsKey(obj) || !obj.CanApplyUpgrade(upgrade)) continue;

            affectedObjects.Add(obj, obj.ApplyUpgrade(upgrade));
        }
    }

    private IUpgrade createUpgrade() => Upgrade.FromEffects(createUpgradeEffect());

    private ParameterModifiableWithId createUpgradeEffect()
    {
        var modificationFactor =
            Parameters.IsReduction ? 1 - Parameters.ModificationValue : Parameters.ModificationValue;
        return new ParameterModifiableWithId(
            Parameters.AttributeAffected,
            new ModificationWithId(modificationId, Modification.MultiplyWith(modificationFactor)),
            UpgradePrerequisites.Empty);
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
