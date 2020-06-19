using System;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities.Collections;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;
using static Bearded.TD.UI.Factories.LegacyDefault;
using static Bearded.TD.UI.Controls.GameDebugOverlay;

namespace Bearded.TD.UI.Controls
{
    class GameDebugOverlayControl : DefaultRenderLayerControl
    {
        private bool minimized;

        public GameDebugOverlayControl(GameDebugOverlay model)
        {
            Add(new BackgroundBox {Color = Color.Purple * 0.5f});

            Add(new Label {Text = "Debug", FontSize = 16, TextAnchor = new Vector2d(0, 0.5)}
                .Anchor(a => a.Top(4, 16).Left(4, 16)));

            Add(Button("x")
                .Anchor(a => a.Top(4, 16).Right(4, 16))
                .Subscribe(b => b.Clicked += model.Close));

            Add(Button("-")
                .Anchor(a => a.Top(4, 16).Right(4 + 16 + 4, 16))
                .Subscribe(b => b.Clicked += toggleMinimized));

            var itemList = new ListControl(new ViewportClippingLayerControl())
                    {ItemSource = new ActionListItemSource(model.Items)}
                .Anchor(a => a.Top(4 + 16 + 4).Right(4).Left(4).Bottom(4));
            Add(itemList);

            toggleMinimized();

            model.ItemsChanged += itemList.Reload;
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
            private readonly ReadOnlyCollection<Item> items;

            public ActionListItemSource(ReadOnlyCollection<Item> items)
            {
                this.items = items;
            }

            public double HeightOfItemAt(int index) => items[index] switch
            {
                OptionsSetting _ => 40,
                _ => 20
            };

            public Control CreateItemControlFor(int index)
            {
                return items[index] switch
                {
                    Header header => new Label {Text = header.Name, FontSize = 16},
                    Command command => button(command.Name, command.Call),
                    BoolSetting boolSetting => checkbox(boolSetting.Name, boolSetting.Value, boolSetting.Toggle),
                    OptionsSetting optionsSetting => multiSelect(optionsSetting),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            private static Control multiSelect(OptionsSetting setting)
            {
                var optionWidthPercentage = 1.0 / setting.Options.Count;

                var container = new CompositeControl
                {
                    new Border(),
                    new Label($"{setting.Name}: {setting.Value}")
                    {
                        TextAnchor = new Vector2d(0, 0.5),
                        FontSize = 16
                    }.Anchor(a => a.Left(4).Bottom(relativePercentage: 0.5))
                };
                setting.Options
                    .Select((o, i) => Button(o.ToString()).Subscribe(
                            b =>
                            {
                                b.FirstChildOfType<Label>().FontSize = 16;
                                b.Clicked += () => setting.Set(i);

                                if (setting.Value.ToString() == o.ToString())
                                {
                                    b.IsEnabled = false;
                                    b.Add(new BackgroundBox(Color.White * 0.5f));
                                }
                            }).Anchor(a => a
                            .Top(relativePercentage: 0.5)
                            .Left(relativePercentage: i * optionWidthPercentage)
                            .Right(relativePercentage: (i + 1) * optionWidthPercentage))
                    ).ForEach(container.Add);

                return container;
            }

            private static Button checkbox(string text, bool value, Action onClick)
                => Button(text).Subscribe(b =>
                {
                    var label = b.FirstChildOfType<Label>();
                    label.TextAnchor = new Vector2d(0, 0.5);
                    label.FontSize = 16;
                    label.Anchor(a => a.Left(4 + 16));

                    b.Add(
                        new BackgroundBox(Color.White * (value ? 0.75f : 0)) {new Border()}
                            .Anchor(a => a.Left(3, 20 - 3 - 3).Top(3).Bottom(3))
                    );

                    b.Clicked += () => onClick();
                });

            private static Button button(string text, Action onClick)
                => Button(text).Subscribe(b =>
                {
                    var label = b.FirstChildOfType<Label>();
                    label.TextAnchor = new Vector2d(0, 0.5);
                    label.FontSize = 16;
                    label.Anchor(a => a.Left(4));

                    b.Clicked += () => onClick();
                });

            public void DestroyItemControlAt(int index, Control control)
            {
            }

            public int ItemCount => items.Count;
        }
    }
}
