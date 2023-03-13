using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints;

static class TilePresenceObservation
{
    private static readonly TileEventHandler discard = _ => { };

    public static ITilePresenceListener Observe(
        this ITilePresence tilePresence, Action<IEnumerable<Tile>> onTilesChanged)
    {
        var observer = new TilePresenceObserver(onTilesChanged);
        observer.Initialize(tilePresence);
        return observer;
    }

    private sealed class TilePresenceObserver : ITilePresenceListener
    {
        private readonly Action<IEnumerable<Tile>> onChanged;
        private ITilePresence? tilePresence;

        public TilePresenceObserver(Action<IEnumerable<Tile>> onChanged)
        {
            this.onChanged = onChanged;
        }

        public void Initialize(ITilePresence tilePresence)
        {
            if (this.tilePresence != null)
            {
                throw new InvalidOperationException();
            }
            this.tilePresence = tilePresence;
            this.tilePresence.TileAdded += onTileMutation;
            this.tilePresence.TileRemoved += onTileMutation;
            onChanged(this.tilePresence.OccupiedTiles);
        }

        private void onTileMutation(Tile _)
        {
            onChanged(tilePresence?.OccupiedTiles ?? ImmutableArray<Tile>.Empty);
        }

        public void Detach()
        {
            onChanged(ImmutableArray<Tile>.Empty);
            StopListeningImmediately();
        }

        public void StopListeningImmediately()
        {
            if (tilePresence == null)
            {
                return;
            }
            tilePresence.TileAdded -= onTileMutation;
            tilePresence.TileRemoved -= onTileMutation;
            tilePresence = null;
        }
    }

    public static ITilePresenceListener ObserveAdditions(
        this ITilePresence tilePresence, TileEventHandler onAdded)
    {
        return ObserveChanges(tilePresence, onAdded, discard);
    }

    public static ITilePresenceListener ObserveChanges(
        this ITilePresence tilePresence, TileEventHandler onAdded, TileEventHandler onRemoved)
    {
        var observer = new TilePresenceChangesObserver(onAdded, onRemoved);
        observer.Initialize(tilePresence);
        return observer;
    }

    private sealed class TilePresenceChangesObserver : ITilePresenceListener
    {
        private readonly TileEventHandler onAdded;
        private readonly TileEventHandler onRemoved;
        private ITilePresence? tilePresence;

        public TilePresenceChangesObserver(TileEventHandler onAdded, TileEventHandler onRemoved)
        {
            this.onAdded = onAdded;
            this.onRemoved = onRemoved;
        }

        public void Initialize(ITilePresence tilePresence)
        {
            if (this.tilePresence != null)
            {
                throw new InvalidOperationException();
            }
            this.tilePresence = tilePresence;
            this.tilePresence.TileAdded += onAdded;
            this.tilePresence.TileRemoved += onRemoved;
            foreach (var tile in this.tilePresence.OccupiedTiles)
            {
                onAdded(tile);
            }
        }

        public void Detach()
        {
            foreach (var tile in tilePresence?.OccupiedTiles ?? ImmutableArray<Tile>.Empty)
            {
                onRemoved(tile);
            }
            StopListeningImmediately();
        }

        public void StopListeningImmediately()
        {
            if (tilePresence == null)
            {
                return;
            }
            tilePresence.TileAdded -= onAdded;
            tilePresence.TileRemoved -= onRemoved;
            tilePresence = null;
        }
    }
}
