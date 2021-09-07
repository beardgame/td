using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Meta
{
    sealed class PlayerCursors
    {
        private const float playerCursorLightHeight = 2;
        private const float playerCursorLightRadius = 5;

        private const float otherCursorLightHeight = 1;
        private const float otherCursorLightRadius = 2.5f;

        private static readonly TimeSpan timeBetweenSyncs = .05.S();

        private readonly GameInstance game;
        private readonly Dictionary<Player, PlayerCursorData> cursors = new();
        private IBuildingBlueprint? attachedGhost;
        private Instant nextSync;

        public PlayerCursors(GameInstance game)
        {
            this.game = game;
        }

        public void AttachGhost(IBuildingBlueprint blueprint)
        {
            attachedGhost = blueprint;
        }

        public void DetachGhost()
        {
            attachedGhost = null;
        }

        public void SyncPlayerCursorPosition(Player p, Position2 pos, IBuildingBlueprint? blueprint)
        {
            if (!cursors.TryGetValue(p, out var currentData))
            {
                cursors[p] = new PlayerCursorData(pos, game.State.Time, pos, blueprint);
                return;
            }
            cursors[p] = currentData with
            {
                LastSyncedLocation = pos,
                LastSyncedTime = game.State.Time,
                LocationAtLastSyncTime = currentData.LocationAtTime(game.State.Time),
                AttachedGhost = blueprint,
            };
        }

        public void Update()
        {
            if (game.State == null) return;

            syncCursors();
            updateGhosts();
        }

        private void syncCursors()
        {
            if (game.State.Time < nextSync) return;

            game.Request(SyncCursors.Request(game, game.Me, game.PlayerInput.CursorPosition, attachedGhost));
            game.State.Meta.Dispatcher.RunOnlyOnServer(
                SyncCursors.Command,
                game,
                cursors.ToImmutableDictionary(
                    pair => pair.Key, pair => (pair.Value.LastSyncedLocation, pair.Value.AttachedGhost)));
            nextSync = game.State.Time + timeBetweenSyncs;
        }

        private void updateGhosts()
        {
            // Use the key collection to iterate since we will be modifying the collection.
            var players = cursors.Keys.ToImmutableArray();

            foreach (var player in players)
            {
                // TODO: why are we treating the player cursor differently?
                if (player == game.Me) continue;

                var cursor = cursors[player];

                switch (cursor.AttachedGhost, cursor.InstantiatedGhost)
                {
                    case ({ } notYetInstantiatedGhost, null):
                        var ghost = new BuildingFactory(game.State)
                            .CreateGhost(notYetInstantiatedGhost, player.Faction, out var tileOccupation);
                        cursor = cursor with
                        {
                            InstantiatedGhost = new InstantiatedGhost(
                                ghost,
                                TileSelection.FromFootprints(notYetInstantiatedGhost.GetFootprintGroup()),
                                tileOccupation)
                        };
                        cursors[player] = cursor;
                        break;
                    case (null, { } stillInstantiatedGhost):
                        stillInstantiatedGhost.Ghost.Delete();
                        cursor = cursor with { InstantiatedGhost = null };
                        cursors[player] = cursor;
                        break;
                }

                if (cursor.InstantiatedGhost != null)
                {
                    var footprint =
                        cursor.InstantiatedGhost.Selection.GetPositionedFootprint(
                            cursor.LocationAtTime(game.State.Time));
                    cursor.InstantiatedGhost!.TileOccupation.SetFootprint(footprint);
                }
            }
        }

        public void DrawCursors(CoreDrawers drawers)
        {
            drawers.PointLight.Draw(
                game.PlayerInput.CursorPosition.NumericValue.WithZ(playerCursorLightHeight),
                radius: playerCursorLightRadius,
                color: Color.White * 0.4f
            );

            foreach (var (player, cursor) in cursors)
            {
                if (player == game.Me) continue;
                drawers.PointLight.Draw(
                    cursor.LocationAtTime(game.State.Time).NumericValue.WithZ(otherCursorLightHeight),
                    radius: otherCursorLightRadius,
                    color: player.Color
                );
            }
        }

        private sealed record PlayerCursorData(
            Position2 LastSyncedLocation,
            Instant LastSyncedTime,
            Position2 LocationAtLastSyncTime,
            IBuildingBlueprint? AttachedGhost,
            InstantiatedGhost? InstantiatedGhost = null)
        {
            public Position2 LocationAtTime(Instant time) =>
                Position2.Lerp(
                    LocationAtLastSyncTime, LastSyncedLocation, (float) ((time - LastSyncedTime) / timeBetweenSyncs));
        }

        private sealed record InstantiatedGhost(
            BuildingGhost Ghost, TileSelection Selection, MovableTileOccupation<BuildingGhost> TileOccupation);
    }
}
