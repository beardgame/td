using System.Collections.ObjectModel;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls
{
    sealed class PlayerStatusUIControl : CompositeControl
    {
        private readonly ListControl listControl;

        public PlayerStatusUIControl(PlayerStatusUI model)
        {
            Add(new BackgroundBox());

            listControl = new ListControl {ItemSource = new PlayerListItemSource(model.Players)};
            Add(listControl.Anchor(a => a.Left(margin: 8).Right(margin: 8)));

            model.PlayersChanged += updatePlayerList;
        }

        private void updatePlayerList()
        {
            listControl.Reload();
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        private class PlayerListItemSource : IListItemSource
        {
            private readonly ReadOnlyCollection<PlayerStatusUI.PlayerModel> players;

            public int ItemCount => players.Count;

            public PlayerListItemSource(ReadOnlyCollection<PlayerStatusUI.PlayerModel> players)
            {
                this.players = players;
            }

            public double HeightOfItemAt(int index) => 32;

            public Control CreateItemControlFor(int index) => new PlayerListRow(players[index]);

            public void DestroyItemControlAt(int index, Control control) {}
        }

        private class PlayerListRow : CompositeControl
        {
            public PlayerListRow(PlayerStatusUI.PlayerModel player)
            {
                Add(new Label(player.Name)
                {
                    Color = player.Color, FontSize = 16, TextAnchor = Label.TextAnchorLeft
                }.Anchor(a => a.Right(margin: 48)));
                Add(new DynamicLabel(() => $"{player.Ping}")
                {
                    FontSize = 16, TextAnchor = Label.TextAnchorRight
                });
            }

            protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
        }
    }
}
