using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

sealed class DrawZoneOnSelect : Component, IListener<DrawComponents>
{
    private TileRangeDrawer tileRangeDrawer = null!;
    private SelectionState selectionState;
    private TileAreaBorder areaBorder = TileAreaBorder.From(ImmutableHashSet<Tile>.Empty);

    protected override void OnAdded()
    {
        SelectionListener.Create(
                onFocus: () => selectionState = SelectionState.Focused,
                onFocusReset: () => selectionState = SelectionState.Default,
                onSelect: () => selectionState = SelectionState.Selected,
                onSelectionReset: () => selectionState = SelectionState.Default)
            .Subscribe(Events);
    }

    public override void Activate()
    {
        base.Activate();
        tileRangeDrawer = new TileRangeDrawer(
            Owner.Game,
            () => TileRangeDrawStyle.FromSelectionState(selectionState),
            () => areaBorder,
            new Color(50, 168, 82));

        Owner.GetTilePresence().Observe(updateTiles);

        Events.Subscribe(this);
    }

    private void updateTiles(IEnumerable<Tile> occupiedTiles)
    {
        areaBorder = TileAreaBorder.From(
            occupiedTiles
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
