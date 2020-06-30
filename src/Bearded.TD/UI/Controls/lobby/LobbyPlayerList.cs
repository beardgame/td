using amulware.Graphics;
using Bearded.TD.Game.Players;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    static class LobbyPlayerList
    {
        public const float RowHeight = fontSize + 2 * margin + 2 * padding;
        private const float margin = 2;
        private const float padding = 4;
        private const float fontSize = 20;
        private const float dotSize = 6;

        public sealed class ItemSource : IListItemSource
        {
            private readonly Lobby lobby;

            public int ItemCount => lobby.Players.Count;
            public bool IsCompact { get; set; }

            public ItemSource(Lobby lobby)
            {
                this.lobby = lobby;
            }

            public double HeightOfItemAt(int index) => RowHeight;

            public Control CreateItemControlFor(int index) => IsCompact
                ? (Control) new CompactRow(lobby.Players[index])
                : new FullWidthRow(lobby.Players[index]);

            public void DestroyItemControlAt(int index, Control control)
            {
            }
        }

        private sealed class CompactRow : CompositeControl
        {
            public CompactRow(Player player)
            {
                Add(new BackgroundBox { Color = Color.White * .1f }.Anchor(a => a.Bottom(margin).Top(margin)));

                Add(new DynamicDot(() => getStatusColorForPlayer(player)).Anchor(a => a
                    .Left(padding, width: dotSize)
                    .Top(margin: -.5 * dotSize, relativePercentage: .5, height: dotSize)));
                Add(new Label(player.Name)
                {
                    Color = Color.White, FontSize = fontSize, TextAnchor = Label.TextAnchorLeft
                }.Anchor(a => a
                    .Left(2 * padding + dotSize)
                    .Right(padding)));
            }

            protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
        }

        private sealed class FullWidthRow : CompositeControl
        {
            public FullWidthRow(Player player)
            {
                Add(new BackgroundBox { Color = Color.White * .1f }.Anchor(a => a.Bottom(margin).Top(margin)));

                Add(new DynamicDot(() => getStatusColorForPlayer(player)).Anchor(a => a
                    .Left(padding, width: dotSize)
                    .Top(margin: -.5 * dotSize, relativePercentage: .5, height: dotSize)));
                Add(new Label(player.Name)
                {
                    Color = Color.White, FontSize = fontSize, TextAnchor = Label.TextAnchorLeft
                }.Anchor(a => a
                    .Left(2 * padding + dotSize)
                    .Right(relativePercentage: .5)));

                Add(new DynamicLabel(() => getStatusStringForPlayer(player), () => getStatusColorForPlayer(player))
                {
                    FontSize = fontSize, TextAnchor = Label.TextAnchorLeft
                }.Anchor(a => a
                    .Left(relativePercentage: .5)
                    .Right(relativePercentage: .75)));
                Add(new DynamicLabel(() => player.LastKnownPing.ToString())
                {
                    Color = Color.White, FontSize = fontSize, TextAnchor = Label.TextAnchorLeft
                }.Anchor(a => a
                    .Left(relativePercentage: .75)
                    .Right(padding)));
            }

            protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
        }

        private static string getStatusStringForPlayer(Player player)
        {
            switch (player.ConnectionState)
            {
                case PlayerConnectionState.Connecting:
                    return "connecting";
                case PlayerConnectionState.Waiting:
                    return "not ready";
                case PlayerConnectionState.LoadingMods:
                    return "loading mods";
                case PlayerConnectionState.Ready:
                    return "ready";
                default:
                    return "unknown";
            }
        }

        private static Color getStatusColorForPlayer(Player player)
        {
            switch (player.ConnectionState)
            {
                case PlayerConnectionState.Connecting:
                    return Color.LightBlue;
                case PlayerConnectionState.Waiting:
                    return Color.Gold;
                case PlayerConnectionState.LoadingMods:
                    return Color.LightBlue;
                case PlayerConnectionState.Ready:
                    return Color.LimeGreen;
                default:
                    return Color.HotPink;
            }
        }
    }
}
