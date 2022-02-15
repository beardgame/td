using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

sealed class DrawZoneOnSelect<T> : Component<T>, IListener<DrawComponents> where T : IGameObject, IComponentOwner
{
    private readonly OccupiedTilesTracker occupiedTilesTracker = new();
    private TileRangeDrawer tileRangeDrawer = null!;
    private SelectionState selectionState;
    private TileAreaBorder areaBorder = TileAreaBorder.From(ImmutableHashSet<Tile>.Empty);

    protected override void OnAdded()
    {
        occupiedTilesTracker.Initialize(Owner, Events);
        tileRangeDrawer = new TileRangeDrawer(
            Owner.Game,
            () => TileRangeDrawStyle.FromSelectionState(selectionState),
            () => areaBorder,
            new Color(50, 168, 82));
        SelectionListener.Create(
                onFocus: () => selectionState = SelectionState.Focused,
                onFocusReset: () => selectionState = SelectionState.Default,
                onSelect: () => selectionState = SelectionState.Selected,
                onSelectionReset: () => selectionState = SelectionState.Default)
            .Subscribe(Events);

        occupiedTilesTracker.TileAdded += _ => updateTiles();
        occupiedTilesTracker.TileRemoved += _ => updateTiles();
        updateTiles();

        Events.Subscribe(this);
    }

    private void updateTiles()
    {
        areaBorder = TileAreaBorder.From(
            occupiedTilesTracker.OccupiedTiles
                .Select(t => Owner.Game.ZoneLayer.ZoneForTile(t))
                .NotNull()
                .Distinct()
                .SelectMany(z => z.CoreTiles));
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(DrawComponents @event)
    {
        tileRangeDrawer.Draw();
    }
}