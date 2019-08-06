using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.UI.Layers;
using Bearded.UI;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using OpenTK;
using static Bearded.TD.UI.Controls.Default;

namespace Bearded.TD.UI.Controls
{
    class UIDebugOverlayControl : DefaultRenderLayerControl
    {
        public class Highlight : Control
        {
            public string Name { get; private set; }
            public float Alpha { get; private set; }
            public double TextY { get; private set; }

            public double Bind(Control control, int number, int totalNumber, double previousTextY)
            {
                var frame = control.Frame;
                this.Anchor(a => a.Left(frame.X.Start, frame.X.Size).Top(frame.Y.Start, frame.Y.Size));

                Name = control.GetType().Name;

                Alpha = (float) (number + totalNumber) / (totalNumber * 2);

                TextY = Math.Max(previousTextY + 10, frame.Y.Start);

                return TextY;
            }
            
            protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);

        }

        private const double margin = 4;
        private const double buttonDimension = 20;
        private const double fontSize = 16;
        private const double controlBoxWidth = 100;
        private const double controlBoxHeight = 100;
        
        private readonly UIDebugOverlay model;
        
        private readonly CompositeControl highlightParent;
        private readonly List<Highlight> highlights = new List<Highlight>();
        private readonly CompositeControl controlBox;
        
        private bool moveControlBox;

        public UIDebugOverlayControl(UIDebugOverlay model)
        {
            this.model = model;
            
            Add(new BackgroundBox { Color = Color.DarkCyan * 0.2f });
            
            Add(highlightParent = new CompositeControl());

            controlBox = new CompositeControl
            {
                new BackgroundBox(),
                Button("move", fontSize)
                    .Anchor(a => a.Top(margin, buttonDimension).Right(margin + buttonDimension + margin).Left(margin))
                    .Subscribe(b => b.Clicked += toggleMoveControlBox),
                Button("x", fontSize)
                    .Anchor(a => a.Top(margin, buttonDimension).Right(margin, buttonDimension))
                    .Subscribe(b => b.Clicked += model.Close)
            };
            controlBox.MouseMove += args => args.Handled = true;
            
            Add(controlBox.Anchor(a => a.Bottom(margin, controlBoxHeight).Left(margin, controlBoxWidth)));
        }
        
        protected override void RenderAsLayerBeforeAncestorLayer(IRendererRouter router)
        {
            SkipNextRender();
        }
        
        protected override void RenderAsLayerAfterAncestorLayer(IRendererRouter router)
        {
            RenderAsLayer(router);
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

        public override void MouseMoved(MouseEventArgs eventArgs)
        {
            renderControlChainFor(eventArgs.MousePosition);
        }

        private void renderControlChainFor(Vector2d point)
        {
            var controlChain = getControlsAt(point);
            showControlChain(controlChain);
        }
        
        private List<Control> getControlsAt(Vector2d point)
        {
            var root = getRootControl();
            
            var chain = new List<Control>();

            object current = root;

            while (current is IControlParent parent)
            {
                var child = parent.Children
                    .LastOrDefault(c => c != this && c.IsVisible && frameContainsPoint(c.Frame, point));

                if (child == null)
                    break;

                chain.Add(child);

                current = child;
            }

            return chain;
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

            return parent;
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


        private void toggleMoveControlBox()
        {
            moveControlBox = !moveControlBox;
        }
    }
}
