using Bearded.Graphics;
using Bearded.TD.Game.Players;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using static Bearded.TD.Constants.UI.Text;
using static Bearded.TD.UI.Factories.TextFactories;

namespace Bearded.TD.UI.Controls;

static class LobbyPlayerList
{
    private const float rowHeight = FontSize + 2 * margin + 2 * padding;
    private const float margin = 2;
    private const float padding = 4;
    private const float dotSize = 6;

    public sealed class ItemSource(Lobby lobby) : IListItemSource
    {
        public int ItemCount => lobby.Players.Count;
        public bool IsCompact { get; set; }

        public double HeightOfItemAt(int index) => rowHeight;

        public Control CreateItemControlFor(int index) => IsCompact
            ? new CompactRow(lobby.Players[index])
            : new FullWidthRow(lobby.Players[index]);

        public void DestroyItemControlAt(int index, Control control) { }
    }

    private sealed class CompactRow : CompositeControl
    {
        public CompactRow(IReadonlyBinding<Lobby.PlayerUIState> player)
        {
            Add(new BackgroundBox { Color = Color.White * .1f }.Anchor(a => a.Bottom(margin).Top(margin)));

            Add(new DynamicDot(player.Transform(getStatusColorForPlayer)).Anchor(a => a
                .Left(padding, width: dotSize)
                .Top(margin: -.5 * dotSize, relativePercentage: .5, height: dotSize)));
            Add(Label(player.Transform(p => p.Name), Label.TextAnchorLeft).Anchor(a => a
                .Left(2 * padding + dotSize)
                .Right(padding)));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }

    private sealed class FullWidthRow : CompositeControl
    {
        public FullWidthRow(IReadonlyBinding<Lobby.PlayerUIState> player)
        {
            Add(new BackgroundBox { Color = Color.White * .1f }.Anchor(a => a.Bottom(margin).Top(margin)));

            Add(new DynamicDot(player.Transform(getStatusColorForPlayer)).Anchor(a => a
                .Left(padding, width: dotSize)
                .Top(margin: -.5 * dotSize, relativePercentage: .5, height: dotSize)));
            Add(Label(player.Transform(p => p.Name), Label.TextAnchorLeft).Anchor(a => a
                .Left(2 * padding + dotSize)
                .Right(relativePercentage: .5)));

            Add(Label(
                    player.Transform(getStatusStringForPlayer),
                    Label.TextAnchorLeft,
                    player.Transform(getStatusColorForPlayer))
                .Anchor(a => a
                    .Left(relativePercentage: .5)
                    .Right(relativePercentage: .75)));
            Add(Label(player.Transform(p => p.LastKnownPing.ToString()), Label.TextAnchorLeft).Anchor(a => a
                .Left(relativePercentage: .75)
                .Right(padding)));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }

    private static string getStatusStringForPlayer(Lobby.PlayerUIState player)
    {
        return player.State switch
        {
            PlayerConnectionState.Connecting => "connecting",
            PlayerConnectionState.Waiting => "not ready",
            PlayerConnectionState.LoadingMods => "loading mods",
            PlayerConnectionState.Ready => "ready",
            _ => "unknown"
        };
    }

    private static Color getStatusColorForPlayer(Lobby.PlayerUIState player)
    {
        return player.State switch
        {
            PlayerConnectionState.Connecting => Color.LightBlue,
            PlayerConnectionState.Waiting => Color.Gold,
            PlayerConnectionState.LoadingMods => Color.LightBlue,
            PlayerConnectionState.Ready => Color.LimeGreen,
            _ => Color.HotPink
        };
    }
}
