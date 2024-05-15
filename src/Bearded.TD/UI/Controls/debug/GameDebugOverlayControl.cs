using System;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities.Collections;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.UI.Controls.GameDebugOverlay;

namespace Bearded.TD.UI.Controls;

sealed class GameDebugOverlayControl : OnTopCompositeControl
{
    private bool minimized;

    public GameDebugOverlayControl(GameDebugOverlay model, UIContext uiContext) : base("Game Debug Overlay")
    {
        var factories = uiContext.Factories;

        Add(new BackgroundBox {Color = Color.Purple * 0.5f});

        Add(new Label {Text = "Debug", FontSize = 16, TextAnchor = new Vector2d(0, 0.5)}
            .Anchor(a => a.Top(4, 16).Left(4, 16)));

        Add(factories.Button("x")
            .Anchor(a => a.Top(4, 16).Right(4, 16))
            .Subscribe(b => b.Clicked += model.Close));

        Add(factories.Button("-")
            .Anchor(a => a.Top(4, 16).Right(4 + 16 + 4, 16))
            .Subscribe(b => b.Clicked += toggleMinimized));

        var itemList = new ListControl(new ViewportClippingLayerControl("Game Debug Overlay List"))
                {ItemSource = new ActionListItemSource(model.Items, uiContext)}
            .Anchor(a => a.Top(4 + 16 + 4).Right(4).Left(4).Bottom(4));
        Add(itemList);

        toggleMinimized(new Button.ClickEventArgs());

        model.ItemsChanged += itemList.Reload;
    }

    private void toggleMinimized(Button.ClickEventArgs _)
    {
        minimized = !minimized;

        const int expandedHeight = 400;
        const int minimizedHeight = 4 + 16 + 4;

        var height = minimized ? minimizedHeight : expandedHeight;
        this.Anchor(a => a
            .Bottom(margin: 30 + expandedHeight - height, height: height)
            .Right(width: 200));
    }

    private sealed class ActionListItemSource : IListItemSource
    {
        private readonly ReadOnlyCollection<Item> items;
        private readonly UIContext uiContext;

        public ActionListItemSource(ReadOnlyCollection<Item> items, UIContext uiContext)
        {
            this.items = items;
            this.uiContext = uiContext;
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
                IBoolSetting boolSetting => checkbox(boolSetting.Name, boolSetting.Value, boolSetting.Toggle),
                OptionsSetting optionsSetting => multiSelect(optionsSetting),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Control multiSelect(OptionsSetting setting)
        {
            var optionWidthPercentage = 1.0 / setting.Options.Length;

            var container = new CompositeControl
            {
                new Border(),
                new Label
                {
                    Text = $"{setting.Name}: {setting.Value}",
                    TextAnchor = new Vector2d(0, 0.5),
                    FontSize = 16
                }.Anchor(a => a.Left(4).Bottom(relativePercentage: 0.5))
            };
            setting.Options
                .Select((o, i) => uiContext.Factories.Button(o.ToString() ?? "").Subscribe(
                        b =>
                        {
                            b.FirstChildOfType<Label>()!.FontSize = 16;
                            b.Clicked += _ => setting.Set(i);

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

        private Button checkbox(string text, bool value, Action onClick)
            => uiContext.Factories.Button(text).Subscribe(b =>
            {
                var label = b.FirstChildOfType<Label>()!;
                label.TextAnchor = new Vector2d(0, 0.5);
                label.FontSize = 16;
                label.Anchor(a => a.Left(4 + 16));

                b.Add(
                    new BackgroundBox(Color.White * (value ? 0.75f : 0)) {new Border()}
                        .Anchor(a => a.Left(3, 20 - 3 - 3).Top(3).Bottom(3))
                );

                b.Clicked += _ => onClick();
            });

        private Button button(string text, Action onClick)
            => uiContext.Factories.Button(text).Subscribe(b =>
            {
                var label = b.FirstChildOfType<Label>()!;
                label.TextAnchor = new Vector2d(0, 0.5);
                label.FontSize = 16;
                label.Anchor(a => a.Left(4));

                b.Clicked += _ => onClick();
            });

        public void DestroyItemControlAt(int index, Control control)
        {
        }

        public int ItemCount => items.Count;
    }
}
