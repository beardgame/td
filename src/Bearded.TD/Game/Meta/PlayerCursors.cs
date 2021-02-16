using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Synchronization;
using Bearded.TD.Game.Players;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
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

        private static readonly TimeSpan timeBetweenSyncs = .1.S();

        private readonly GameInstance game;
        private readonly Dictionary<Player, Tile> latestKnownCursorPosition = new Dictionary<Player, Tile>();
        private Instant nextUpdate;

        public PlayerCursors(GameInstance game)
        {
            this.game = game;
        }

        public void SetPlayerCursorPosition(Player p, Tile t)
        {
            latestKnownCursorPosition[p] = t;
        }

        public void Update()
        {
            if (game.State == null || game.State.Time < nextUpdate) return;

            game.Request(SyncCursors.Request(game, game.Me, Level.GetTile(game.PlayerInput.CursorPosition)));
            game.State.Meta.Dispatcher.RunOnlyOnServer(
                SyncCursors.Command, game, ImmutableDictionary.CreateRange(latestKnownCursorPosition));
            nextUpdate = game.State.Time + timeBetweenSyncs;
        }

        public void DrawCursors(CoreDrawers drawers)
        {
            drawers.PointLight.Draw(
                game.PlayerInput.CursorPosition.NumericValue.WithZ(playerCursorLightHeight),
                radius: playerCursorLightRadius,
                color: Color.White * 0.4f
            );

            foreach (var (player, tile) in latestKnownCursorPosition)
            {
                if (player == game.Me) continue;
                drawers.PointLight.Draw(
                    Level.GetPosition(tile).NumericValue.WithZ(otherCursorLightHeight),
                    radius: otherCursorLightRadius,
                    color: player.Color
                );
            }
        }
    }
}
