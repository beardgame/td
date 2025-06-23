using Bearded.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI.Text;
using static Bearded.TD.UI.Factories.TextFactories;

namespace Bearded.TD.UI.Controls;

sealed class LobbyListRowControl : CompositeControl
{
    public const float Height = FontSize + 2 * margin + 2 * padding;
    private const float margin = 2;
    private const float padding = 4;

    public GenericEventHandler<Proto.Lobby>? Clicked;

    public LobbyListRowControl(Proto.Lobby lobby)
    {
        Add(new BackgroundBox { Color = Color.White * .1f }.Anchor(a => a.Bottom(margin).Top(margin)));

        Add(new Button
        {
            Label(lobby.Name, Label.TextAnchorLeft)
                .Anchor(a => a
                    .Left(padding)
                    .Right(relativePercentage: .5)),
            Label($"{lobby.CurrentNumPlayers} / {lobby.MaxNumPlayers}", Label.TextAnchorRight)
                .Anchor(a => a
                    .Left(relativePercentage: .5)
                    .Right(padding)),
        }.Subscribe(btn => btn.Clicked += _ => Clicked?.Invoke(lobby)));
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
