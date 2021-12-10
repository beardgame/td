using Bearded.Graphics;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class PerformanceOverlayControl : CompositeControl
{
    public PerformanceOverlayControl(PerformanceOverlay model)
    {
        Add(new BackgroundBox { Color = Color.Blue * 0.5f });

        Add(new Label { Text = $"Frame time ({model.FramesConsideredForAverage} frame avg)", FontSize = 14, TextAnchor = new Vector2d(0, 0.5) }
            .Anchor(a => a.Top(4, 16).Left(4, 16)));

        var itemList = new ListControl(new ViewportClippingLayerControl())
                { ItemSource = new FrameTimeListItemSource(model) }
            .Anchor(a => a.Top(4 + 16 + 4).Right(4).Left(4).Bottom(4));
        Add(itemList);

        model.LastFrameUpdated += itemList.Reload;

        this.Anchor(a => a
            .Bottom(margin: 10, height: 100)
            .Left(margin: 10, width: 200));
    }

    private sealed class FrameTimeListItemSource : IListItemSource
    {
        private readonly PerformanceOverlay model;

        public FrameTimeListItemSource(PerformanceOverlay model)
        {
            this.model = model;
        }

        private const double itemHeight = 12;

        public double HeightOfItemAt(int index) => itemHeight;

        public Control CreateItemControlFor(int index)
        {
            var item = model.LastFrame[index];

            return
                new CompositeControl
                {
                    new Label
                    {
                        Text = $"{item.Name}:",
                        TextAnchor = new Vector2d(0, 0.5),
                        FontSize = itemHeight,
                    },
                    new Label
                    {
                        Text = formatTime(item.Time),
                        TextAnchor = new Vector2d(1, 0.5),
                        FontSize = itemHeight,
                        Color = index == 0 ? colorForTotalTime(item.Time) : colorForTime(item.Time),
                    },
                };
        }

        private static Color colorForTotalTime(TimeSpan time)
        {
            var seconds = time.NumericValue;
            var milliseconds = (float)seconds * 1000;

            return milliseconds switch
            {
                >= 10 => Color.Red,
                >= 5 => Color.Lerp(Color.Yellow, Color.Red, (milliseconds - 5) / (10 - 5)),
                >= 1 => Color.Lerp(Color.Green, Color.Yellow, (milliseconds - 1) / (5 - 1)),
                _ => Color.Green
            };
        }

        private static Color colorForTime(TimeSpan time)
        {
            var seconds = time.NumericValue;
            var milliseconds = (float)seconds * 1000;

            return milliseconds switch
            {
                >= 10 => Color.Red,
                >= 1 => Color.Lerp(Color.Orange, Color.Red, (milliseconds - 1) / (10 - 1)),
                >= 0.1f => Color.Lerp(Color.Yellow, Color.Orange, (milliseconds - 0.1f) / (1 - 0.1f)),
                >= 0.01f => Color.Lerp(Color.Gray, Color.Yellow, (milliseconds - 0.01f) / (0.1f - 0.01f)),
                _ => Color.Gray
            };
        }

        private static string formatTime(TimeSpan time)
        {
            var seconds = time.NumericValue;
            var milliseconds = seconds * 1000;
            var microseconds = milliseconds * 1000;

            return milliseconds switch
            {
                >= 1000 => $"{seconds:0.00}s",
                >= 100 => $"{milliseconds:0}ms",
                >= 10 => $"{milliseconds:0.0}ms",
                >= 1 => $"{milliseconds:0.00}ms",
                >= 0.1 => $"{microseconds:0}us",
                >= 0.01 => $"{microseconds:0.0}us",
                >= 0.001 => $"{microseconds:0.00}us",
                _ => "<1us",
            };
        }

        public void DestroyItemControlAt(int index, Control control)
        {
        }

        public int ItemCount => model.LastFrame.Length;
    }
}