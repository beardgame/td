using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Input;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Meta
{
    sealed class PlayerCursors
    {
        private const float playerCursorLightHeight = 2;
        private const float playerCursorLightRadius = 5;

        private const float otherCursorLightHeight = 1;
        private const float otherCursorLightMinRadius = 2.5f;
        private const float otherCursorLightMaxRadius = 5f;
        private const float otherCursorLightMinAlpha = .5f;
        private const float otherCursorLightMaxAlpha = .9f;

        private const float momentumFactor = 1E-5f;
        private const float momentumDecayPerSecond = .99999f;

        private static readonly TimeSpan timeBetweenSyncs = .05.S();

        private readonly GameInstance game;
        private readonly Dictionary<Player, PlayerCursorData> cursors = new();
        private IComponentOwnerBlueprint? attachedGhost;
        private Instant currentTime = Instant.Zero;
        private Instant nextSync;

        public PlayerCursors(GameInstance game)
        {
            this.game = game;
        }

        public void AttachGhost(IComponentOwnerBlueprint blueprint)
        {
            attachedGhost = blueprint;
        }

        public void DetachGhost()
        {
            attachedGhost = null;
        }

        public void SyncPlayerCursorPosition(Player p, Position2 pos, IComponentOwnerBlueprint? blueprint)
        {
            if (!cursors.TryGetValue(p, out var currentData))
            {
                cursors[p] = new PlayerCursorData(pos, 0f, currentTime, pos, 0f, Velocity2.Zero, blueprint);
                return;
            }

            var timeSinceLastSync = currentTime - currentData.LastSyncedTime;
            var velocitySinceLastSync = timeSinceLastSync == TimeSpan.Zero
                ? currentData.VelocityBeforeLastSyncTime
                : (pos - currentData.LastSyncedLocation) / timeSinceLastSync;

            // Decay
            var decay = MathF.Pow(1 - momentumDecayPerSecond, (float)timeSinceLastSync.NumericValue);
            var momentum = MathF.Max(0, currentData.LastSyncedMomentum * decay);
            // New energy
            if (timeSinceLastSync > TimeSpan.Zero)
            {
                var averageVelocitySquared =
                    (0.5f * (velocitySinceLastSync.Length + currentData.VelocityBeforeLastSyncTime.Length)).Squared;

                var oldDirection = currentData.VelocityBeforeLastSyncTime.Direction;
                var newDirection = velocitySinceLastSync.Direction;
                var angularVelocity = (newDirection - oldDirection).Abs() / timeSinceLastSync;

                momentum += (float)timeSinceLastSync.NumericValue * momentumFactor *
                    averageVelocitySquared.NumericValue * angularVelocity.NumericValue;
            }

            cursors[p] = currentData with
            {
                LastSyncedLocation = pos,
                LastSyncedMomentum = momentum,
                LastSyncedTime = currentTime,
                LocationAtLastSyncTime = currentData.LocationAtTime(currentTime),
                MomentumAtLastSyncTime = currentData.MomentumAtTime(currentTime),
                VelocityBeforeLastSyncTime = velocitySinceLastSync,
                AttachedGhost = blueprint,
            };
        }

        public void Update(UpdateEventArgs updateEventArgs)
        {
            if (game.State == null) return;

            currentTime += updateEventArgs.ElapsedTimeInS.S();

            syncCursors();
            updateGhosts();
        }

        private void syncCursors()
        {
            if (currentTime < nextSync) return;

            game.Request(SyncCursors.Request(game, game.Me, game.PlayerInput.CursorPosition, attachedGhost));
            game.State.Meta.Dispatcher.RunOnlyOnServer(
                SyncCursors.Command,
                game,
                cursors.ToImmutableDictionary(
                    pair => pair.Key, pair => (pair.Value.LastSyncedLocation, pair.Value.AttachedGhost)));
            nextSync = currentTime + timeBetweenSyncs;
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
                        cursor.InstantiatedGhost.Selection.GetPositionedFootprint(cursor.LocationAtTime(currentTime));
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
                if (player == game.Me)
                {
                    continue;
                }

                var pos = cursor.LocationAtTime(currentTime);
                var momentum = cursor.MomentumAtTime(currentTime);

                drawers.PointLight.Draw(
                    pos.NumericValue.WithZ(otherCursorLightHeight),
                    radius: radiusForMomentum(momentum),
                    color: player.Color * alphaForMomentum(momentum)
                );
            }
        }

        private static float radiusForMomentum(float momentum) =>
            otherCursorLightMinRadius +
            lerpFactorForMomentum(momentum) * (otherCursorLightMaxRadius - otherCursorLightMinRadius);

        private static float alphaForMomentum(float momentum) =>
            otherCursorLightMinAlpha +
            lerpFactorForMomentum(momentum) * (otherCursorLightMaxAlpha - otherCursorLightMinAlpha);

        private static float lerpFactorForMomentum(float momentum) => MathF.Max(0, 1 - 1 / (1 + momentum));

        private sealed record PlayerCursorData(
            Position2 LastSyncedLocation,
            float LastSyncedMomentum,
            Instant LastSyncedTime,
            Position2 LocationAtLastSyncTime,
            float MomentumAtLastSyncTime,
            Velocity2 VelocityBeforeLastSyncTime,
            IComponentOwnerBlueprint? AttachedGhost,
            InstantiatedGhost? InstantiatedGhost = null)
        {
            public Position2 LocationAtTime(Instant time) =>
                Position2.Lerp(
                    LocationAtLastSyncTime, LastSyncedLocation, (float) ((time - LastSyncedTime) / timeBetweenSyncs));

            public float MomentumAtTime(Instant time) =>
                Interpolate.Lerp(
                    MomentumAtLastSyncTime, LastSyncedMomentum, (float)((time - LastSyncedTime) / timeBetweenSyncs));
        }

        private sealed record InstantiatedGhost(
            ComponentGameObject Ghost,
            TileSelection Selection,
            MovableTileOccupation<ComponentGameObject> TileOccupation);
    }
}
