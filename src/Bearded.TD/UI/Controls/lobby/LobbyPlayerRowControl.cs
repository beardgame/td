using amulware.Graphics;
using Bearded.TD.Game.Players;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyPlayerRowControl : CompositeControl
    {
        public const float Height = fontSize + 2 * margin + 2 * padding;
        private const float margin = 2;
        private const float padding = 4;
        private const float fontSize = 20;

        public LobbyPlayerRowControl(Player player)
        {
            Add(new BackgroundBox { Color = Color.White * .1f }.Anchor(a => a.Bottom(margin).Top(margin)));

            Add(new Label(player.Name)
            {
                Color = Color.White, FontSize = fontSize, TextAnchor = Label.TextAnchorLeft
            }.Anchor(a => a
                .Left(padding)
                .Right(relativePercentage: .5)));

            Add(new DynamicLabel(() => getStatusStringForPlayer(player))
            {
                Color = Color.White, FontSize = fontSize, TextAnchor = Label.TextAnchorLeft
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

        private static string getStatusStringForPlayer(Player player)
            => player.ConnectionState == PlayerConnectionState.Ready ? "ready" : "not ready";
    }
}
