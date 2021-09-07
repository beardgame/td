using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Players;
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
        private Instant nextUpdate;

        public PlayerCursors(GameInstance game)
        {
            this.game = game;
        }

        public void SetPlayerCursorPosition(Player p, Position2 pos)
        {
            if (!cursors.TryGetValue(p, out var currentData))
            {
                cursors[p] = new PlayerCursorData(pos, game.State.Time, pos);
                return;
            }
            cursors[p] = currentData with
            {
                LastSyncedLocation = pos,
                LastSyncedTime = game.State.Time,
                LocationAtLastSyncTime = currentData.LocationAtTime(game.State.Time),
            };
        }

        public void Update()
        {
            if (game.State == null || game.State.Time < nextUpdate) return;

            game.Request(SyncCursors.Request(game, game.Me, game.PlayerInput.CursorPosition));
            game.State.Meta.Dispatcher.RunOnlyOnServer(
                SyncCursors.Command,
                game,
                cursors.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.LastSyncedLocation));
            nextUpdate = game.State.Time + timeBetweenSyncs;
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
            Position2 LastSyncedLocation, Instant LastSyncedTime, Position2 LocationAtLastSyncTime)
        {
            public Position2 LocationAtTime(Instant time) =>
                Position2.Lerp(
                    LocationAtLastSyncTime, LastSyncedLocation, (float) ((time - LastSyncedTime) / timeBetweenSyncs));
        }
    }
}
