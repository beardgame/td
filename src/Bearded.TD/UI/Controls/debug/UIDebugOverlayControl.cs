using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.UI;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MouseButtonEventArgs = Bearded.UI.EventArgs.MouseButtonEventArgs;
using MouseEventArgs = Bearded.UI.EventArgs.MouseEventArgs;

namespace Bearded.TD.UI.Controls;

sealed class UIDebugOverlayControl : OnTopCompositeControl
{
    public sealed class Highlight : Control
    {
        public string Name { get; private set; } = "";
        public bool PassThrough { get; private set; }
        public float Alpha { get; private set; }
        public double TextY { get; private set; }

        public double Bind(Control control, int number, int totalNumber, double previousTextY)
        {
            var frame = control.Frame;
            this.Anchor(a => a.Left(frame.X.Start, frame.X.Size).Top(frame.Y.Start, frame.Y.Size));

            Name = control.GetType().Name;
            PassThrough = control.IsClickThrough;

            Alpha = (float) (number + totalNumber) / (totalNumber * 2);

            TextY = Math.Max(previousTextY + 10, frame.Y.Start);

            return TextY;
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

    }

    private const double margin = 4;
    private const double buttonDimension = 20;
    private const double controlBoxWidth = 200;
    private const double controlBoxHeight = 400;

    private readonly CompositeControl highlightParent;
    private readonly List<Highlight> highlights = new();
    private readonly CompositeControl controlBox;
    private readonly ListControl highlightList  = new();

    private bool moveControlBox;

    public UIDebugOverlayControl(UIDebugOverlay model, UIFactories factories) : base("UI Debug Overlay")
    {
        Add(new BackgroundBox { Color = Color.DarkCyan * 0.2f });

        Add(highlightParent = new CompositeControl());

        controlBox = new CompositeControl
        {
            new BackgroundBox(),
            factories.Button("move")
                .Anchor(a => a.Top(margin, buttonDimension).Right(margin + buttonDimension + margin).Left(margin))
                .Subscribe(b => b.Clicked += toggleMoveControlBox),
            factories.Button("x")
                .Anchor(a => a.Top(margin, buttonDimension).Right(margin, buttonDimension))
                .Subscribe(b => b.Clicked += model.Close),
            highlightList.Anchor(a => a.Top(margin + buttonDimension + margin).Bottom(margin).Left(margin).Right(margin))
        };
        controlBox.MouseMove += args => args.Handled = true;

        Add(controlBox.Anchor(a => a.Bottom(margin, controlBoxHeight).Left(margin, controlBoxWidth)));
    }

    public override void PreviewMouseMoved(MouseEventArgs eventArgs)
    {
        if (!moveControlBox)
            return;

        var p = eventArgs.MousePosition;
        var newX = p.X - controlBox.Frame.X.Size / 2;
        var newY = p.Y - margin - buttonDimension / 2;
        controlBox.Anchor(a => a.Top(newY, controlBoxHeight).Left(newX, controlBoxWidth));

        eventArgs.Handled = true;
    }

    public override void MouseButtonReleased(MouseButtonEventArgs eventArgs)
    {
        if (eventArgs.MouseButton == MouseButton.Left)
        {
            eventArgs.Handled = true;
            var currentControlChain = getControlsAt(eventArgs.MousePosition);
            // ReSharper disable once UnusedVariable
            var currentControl = currentControlChain.LastOrDefault();
            Debugger.Break();
        }
    }

    public override void MouseMoved(MouseEventArgs eventArgs)
    {
        renderControlChainFor(eventArgs.MousePosition);
    }

    private void renderControlChainFor(Vector2d point)
    {
        var controlChain = getControlsAt(point);
        showControlChain(controlChain);
        listControlChain(controlChain);
    }

    private List<Control> getControlsAt(Vector2d point)
    {
        var root = getRootControl();

        var chain = new List<Control>();
        traverse(root);
        chain.Reverse();
        return chain;

        bool traverse(IControlParent parent)
        {
            foreach (var c in parent.Children.Reverse())
            {
                if (c == this) continue;
                if (!c.IsVisible || !frameContainsPoint(c.Frame, point)) continue;

                if (c is IControlParent cParent)
                {
                    if (!traverse(cParent) && c.IsClickThrough)
                    {
                        continue;
                    }
                }
                else if (c.IsClickThrough)
                {
                    continue;
                }

                chain.Add(c);
                return true;
            }

            return false;
        }
    }

    private bool frameContainsPoint(Frame frame, Vector2d point)
    {
        return frame.X.Start < point.X && frame.X.End > point.X && frame.Y.Start < point.Y && frame.Y.End > point.Y;
    }

    private IControlParent getRootControl()
    {
        var parent = Parent;

        while (parent is Control parentControl)
            parent = parentControl.Parent;

        return parent!;
    }

    private void showControlChain(List<Control> controlChain)
    {
        setHighlightCountTo(controlChain.Count);

        var i = 0;
        var previousTextY = double.NegativeInfinity;
        foreach (var (control, highlight) in controlChain.Zip(highlights, (c, h) => (c, h)))
        {
            previousTextY = highlight.Bind(control, i, controlChain.Count, previousTextY);
            i++;
        }
    }

    private void setHighlightCountTo(int controlChainCount)
    {
        while (highlights.Count < controlChainCount)
        {
            var highlight = new Highlight();
            highlights.Add(highlight);
            highlightParent.Add(highlight);
        }

        foreach (var i in Enumerable.Range(controlChainCount, highlights.Count - controlChainCount))
        {
            highlights[i].RemoveFromParent();
        }
        highlights.RemoveRange(controlChainCount, highlights.Count - controlChainCount);
    }

    private void listControlChain(List<Control> controlChain)
    {
        highlightList.ItemSource = new HighlightListItemSource(controlChain);
    }

    private sealed class HighlightListItemSource : IListItemSource
    {
        private readonly List<Control> controls;

        public HighlightListItemSource(List<Control> controls)
        {
            this.controls = controls;
        }

        public double HeightOfItemAt(int index) => 14;

        public Control CreateItemControlFor(int index)
            => new Label
            {
                Text = controls[index].GetType().Name,
                Color = Color.IndianRed,
                FontSize = 14,
                TextAnchor = new Vector2d(0, 0.5)
            };

        public void DestroyItemControlAt(int index, Control control)
        {
        }

        public int ItemCount => controls.Count;
    }


    private void toggleMoveControlBox(Button.ClickEventArgs _)
    {
        moveControlBox = !moveControlBox;
    }
}
