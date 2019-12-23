using System;
using System.Collections.ObjectModel;
using amulware.Graphics;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;
using static Bearded.TD.UI.Controls.DefaultControls;

namespace Bearded.TD.UI.Controls
{
    class GameDebugOverlayControl : DefaultRenderLayerControl
    {
        private bool minimized = true;

        public GameDebugOverlayControl(GameDebugOverlay model)
        {
            Add(new BackgroundBox {Color = Color.Purple * 0.25f});

            Add(new Label {Text = "Debug", FontSize = 16, TextAnchor = new Vector2d(0, 0.5)}
                .Anchor(a => a.Top(4, 16).Left(4, 16)));

            Add(Button("x")
                .Anchor(a => a.Top(4, 16).Right(4, 16))
                .Subscribe(b => b.Clicked += model.Close));

            Add(Button("-")
                .Anchor(a => a.Top(4, 16).Right(4 + 16 + 4, 16))
                .Subscribe(b => b.Clicked += toggleMinimized));

            Add(new ListControl(new ViewportClippingLayerControl())
                    {ItemSource = new ActionListItemSource(model.Items)}
                .Anchor(a => a.Top(4 + 16 + 4).Right(4).Left(4).Bottom(4)));

            toggleMinimized();
        }

        private void toggleMinimized()
        {
            minimized = !minimized;

            var height = minimized ? 4 + 16 + 4 : 400;
            this.Anchor(a => a.Top(10, height).Left(200, 200));
        }

        protected override void RenderAsLayerBeforeAncestorLayer(IRendererRouter router)
        {
            SkipNextRender();
        }

        protected override void RenderAsLayerAfterAncestorLayer(IRendererRouter router)
        {
            RenderAsLayer(router);
        }

        private class ActionListItemSource : IListItemSource
        {
            private readonly ReadOnlyCollection<GameDebugOverlay.Item> items;

            public ActionListItemSource(ReadOnlyCollection<GameDebugOverlay.Item> items)
            {
                this.items = items;
            }

            public double HeightOfItemAt(int index) => 20;

            public Control CreateItemControlFor(int index)
            {
                var item = items[index];

                switch (item)
                {
                    case GameDebugOverlay.Command command:
                        return Button(command.Name)
                            .Subscribe(b => b.Clicked += command.Call)
                            .Subscribe(b =>
                            {
                                var label = b.FirstChildOfType<Label>();
                                label.TextAnchor = new Vector2d(0, 0.5);
                                label.FontSize = 16;
                            });
                    default:
                        throw new ArgumentOutOfRangeException(nameof(item));
                }
            }

            public void DestroyItemControlAt(int index, Control control)
            {
            }

            public int ItemCount => items.Count;
        }
    }
}
