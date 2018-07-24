using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyListRowControl : CompositeControl
    {
        public const float Height = fontSize + 2 * margin + 2 * padding;
        private const float margin = 2;
        private const float padding = 4;
        private const float fontSize = 20;

        public LobbyListRowControl(Proto.Lobby lobby)
        {
            Add(new BackgroundBox { Color = Color.White * .1f }.Anchor(a => a.Bottom(margin).Top(margin)));

            Add(new Label(lobby.Name)
            {
                Color = Color.White, FontSize = fontSize, TextAnchor = Label.TextAnchorLeft
            }.Anchor(a => a
                .Left(padding)
                .Right(relativePercentage: .5)));

            Add(new Label($"{lobby.CurrentNumPlayers} / {lobby.MaxNumPlayers}")
            {
                Color = Color.White, FontSize = fontSize, TextAnchor = Label.TextAnchorRight
            }.Anchor(a => a
                .Left(relativePercentage: .5)
                .Right(padding)));
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
