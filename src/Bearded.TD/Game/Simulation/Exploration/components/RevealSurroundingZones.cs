using System;
using System.Diagnostics.CodeAnalysis;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Exploration;

[Component("revealSurroundingZones")]
sealed class RevealSurroundingZones<T> : Component<T, IRevealSurroundingZonesParameters>
    where T : IGameObject, IComponentOwner<T>
{
    private IFactionProvider? factionProvider;
    private InitializedFaction? initializedFaction;

    public RevealSurroundingZones(IRevealSurroundingZonesParameters parameters) : base(parameters) {}

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
    }

    public override void OnRemoved()
    {
        initializedFaction?.Dispose();
        base.OnRemoved();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (factionProvider?.Faction == initializedFaction?.Faction)
        {
            return;
        }

        initializedFaction?.Dispose();
        initializedFaction = null;
        if (factionProvider != null)
        {
            InitializedFaction.TryCreate(
                Owner, Events, factionProvider.Faction, Parameters, out initializedFaction);
        }
    }

    private sealed class InitializedFaction : IDisposable
    {
        private readonly OccupiedTilesTracker occupiedTilesTracker = new();
        private readonly GameState game;
        private readonly ComponentEvents events;
        private readonly FactionVisibility visibility;
        private readonly IRevealSurroundingZonesParameters parameters;

        public Faction Faction { get; }

        private InitializedFaction(
            GameState game,
            ComponentEvents events,
            Faction faction,
            FactionVisibility visibility,
            IRevealSurroundingZonesParameters parameters)
        {
            this.game = game;
            this.events = events;
            Faction = faction;
            this.visibility = visibility;
            this.parameters = parameters;
            occupiedTilesTracker.TileAdded += onTileAdded;
        }

        private void onTileAdded(Tile t)
        {
            var zone = game.ZoneLayer.ZoneForTile(t);
            if (zone != null)
            {
                recursivelyRevealZones(zone, parameters.Steps);
            }
        }

        private void recursivelyRevealZones(Zone z, int steps)
        {
            if (!visibility.RevealZone(z) || steps <= 0)
            {
                return;
            }

            foreach (var adjacentZone in game.ZoneLayer.AdjacentZones(z))
            {
                recursivelyRevealZones(adjacentZone, steps - 1);
            }
        }

        public static bool TryCreate(
            T owner,
            ComponentEvents events,
            Faction faction,
            IRevealSurroundingZonesParameters parameters,
            [NotNullWhen(true)] out InitializedFaction? result)
        {
            if (!faction.TryGetBehaviorIncludingAncestors<FactionVisibility>(out var visibility))
            {
                result = default;
                return false;
            }
            result = new InitializedFaction(owner.Game, events, faction, visibility, parameters);
            result.occupiedTilesTracker.Initialize(owner, events);
            return true;
        }

        public void Dispose()
        {
            occupiedTilesTracker.Dispose(events);
        }
    }
}

interface IRevealSurroundingZonesParameters : IParametersTemplate<IRevealSurroundingZonesParameters>
{
    [Modifiable(1)]
    public int Steps { get; }
}
