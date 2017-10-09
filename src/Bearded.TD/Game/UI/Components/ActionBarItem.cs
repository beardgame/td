using System;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using OpenTK;

namespace Bearded.TD.Game.UI.Components
{
    class ActionBarItem : FocusableUIComponent
    {
        private static readonly Color bgColor = Color.Black * .4f;
        private static readonly Color focusedBgColor = Color.Black * .8f;
        private static readonly Vector2 bgPadding = 2 * Vector2.One;

        private Content content;

        public ActionBarItem(Bounds bounds) : base(bounds)
        {
            Unfocused += _ => content?.Undo?.Invoke();
        }

        public void SetContent(Content content)
        {
            this.content = content;
        }

        public override void HandleInput(InputContext input)
        {
            base.HandleInput(input);
            if (input.State.Mouse.Click.Hit && Bounds.Contains(input.MousePosition))
            {
                Focus();
                content?.Action();
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            geometries.Primitives.Color = IsFocused ? focusedBgColor : bgColor;
            geometries.Primitives.DrawRectangle(Bounds.Start() + bgPadding, Bounds.Size() - 2 * bgPadding);

            if (content == null) return;

            geometries.ConsoleFont.Color = Color.White;
            geometries.ConsoleFont.Height = Constants.UI.FontSize;
            geometries.ConsoleFont.SizeCoefficient = Vector2.One;
            var stringPos = new Vector2(Bounds.XStart + bgPadding.X + Constants.UI.BoxPadding, Bounds.CenterY());
            geometries.ConsoleFont.DrawString(stringPos, content.Description, 0, .5f);
        }

        public class Content
        {
            public Action Action { get; }
            public Action Undo { get; }
            public string Description { get; }

            public Content(Action action, Action undo, string description)
            {
                Action = action;
                Undo = undo;
                Description = description;
            }
        }
    }
}
