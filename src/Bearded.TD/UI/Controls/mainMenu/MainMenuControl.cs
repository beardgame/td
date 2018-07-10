﻿using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenuControl : CompositeControl
    {
        public MainMenuControl(MainMenu model)
        {
            Add(
                new CompositeControl() // ButtonGroup
                {
                    new Button {new Label("Host game")}
                        .Anchor(a => a.Top(margin: 0, height: 50))
                        .Subscribe(b => b.Clicked += model.OnHostGameButtonClicked),
                    new Button {new Label("Join game")}
                        .Anchor(a => a.Top(margin: 50, height: 50))
                        .Subscribe(b => b.Clicked += model.OnJoinGameButtonClicked),
                    new Button {new Label("Exit")}
                        .Anchor(a => a.Top(margin: 100, height: 50))
                        .Subscribe(b => b.Clicked += model.OnQuitGameButtonClicked)
                }.Anchor(a => a.Right(margin: 20, width: 250).Bottom(margin: 20, height: 150))
            );

            var source = new ListItemSource(25);
            var list = new ListControl(source, new ViewportClippingLayerControl())
                .Anchor(a => a.Bottom(300).Left(0, 100).Top(50));
            source.List = list;

            Add(list);
        }

        class ListItemSource : IListItemSource
        {
            public ListItemSource(int i)
            {
                ItemCount = i;
            }
            
            public int ItemCount { get; }
            public ListControl List { get; set; }

            public double HeightOfItemAt(int index)
            {
                return 23 + index * 3;
            }

            public Control CreateItemControlFor(int index)
            {
                return new Button { new Label(index.ToString()) }
                    .Subscribe(c => c.Clicked += List.ScrollToBottom);
            }

            public void DestroyItemControlAt(int index, Control control)
            {
            }
        }
    }
}
