using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Exploration;

[GameRule("spawnExplorationBeacons")]
sealed class SpawnExplorationBeacons : GameRule<SpawnExplorationBeacons.RuleParameters>
{
    public SpawnExplorationBeacons(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(new Listener(context.GameState, Parameters.Blueprint));
    }

    private sealed class Listener : IListener<ExplorableZonesChanged>
    {
        private readonly GameState gameState;
        private readonly ComponentOwnerBlueprint blueprint;

        private readonly Dictionary<Zone, ComponentGameObject> beaconsByZone = new();

        public Listener(GameState gameState, ComponentOwnerBlueprint blueprint)
        {
            this.gameState = gameState;
            this.blueprint = blueprint;
        }

        public void HandleEvent(ExplorableZonesChanged @event)
        {
            synchronizeBeacons(@event.ExplorableZones);
        }

        private void synchronizeBeacons(IEnumerable<Zone> explorableZones)
        {
            var oldZones = beaconsByZone.Keys.ToImmutableHashSet();
            var newZones = explorableZones.ToImmutableHashSet();

            var zonesAdded = newZones.Except(oldZones);
            var zonesRemoved = oldZones.Except(newZones);

            foreach (var z in zonesAdded)
            {
                var beacon =
                    ExplorationBeaconFactory.CreateExplorationBeacon(gameState, blueprint, determineCenter(z), z);
                beaconsByZone.Add(z, beacon);
            }

            foreach (var z in zonesRemoved)
            {
                var beacon = beaconsByZone[z];
                beacon.Delete();
                beaconsByZone.Remove(z);
            }
        }

        private Tile determineCenter(Zone zone)
        {
            var centroid = new Position2(
                zone.Tiles
                    .Select(tile => Level.GetPosition(tile).NumericValue / zone.Tiles.Length)
                    .Aggregate((v1, v2) => v1 + v2));
            // TODO: remove the NumericValue once we can use the System.Linq MinBy.
            var closestToCentroid =
                zone.Tiles.MinBy(tile => (Level.GetPosition(tile) - centroid).LengthSquared.NumericValue);
            return closestToCentroid;
        }
    }

    public record RuleParameters(ComponentOwnerBlueprint Blueprint);
}