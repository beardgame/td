using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories;

static class MenuFactories
{
    public static Layouts.Layout AddMenu(
        this Layouts.Layout layout, BuilderFunc<Builder> builderFunc)
    {
        var builder = new Builder();
        builderFunc(builder);
        layout.DockFixedSizeToRight(builder.Build(), Constants.UI.Menu.Width);
        return layout;
    }

    public sealed class Builder
    {
        private ButtonAction? closeAction;
        private readonly List<ButtonAction> menuActions = new();
        private Control? background;
        private Animations? animations;

        public Builder WithAnimations(Animations? animations)
        {
            this.animations = animations;
            return this;
        }

        public Builder WithCloseAction(VoidEventHandler onClose) => WithCloseAction("Close", onClose);

        public Builder WithCloseAction(string label, VoidEventHandler onClose, Binding<bool>? isEnabled = null)
        {
            closeAction = new ButtonAction(label, onClose, isEnabled);
            return this;
        }

        public Builder AddMenuAction(string label, VoidEventHandler onClick, Binding<bool>? isEnabled = null)
        {
            menuActions.Add(new ButtonAction(label, onClick, isEnabled));
            return this;
        }

        public Builder WithBackground(Control background)
        {
            this.background = background;
            return this;
        }

        public Control Build()
        {
            if (closeAction == null)
            {
                throw new InvalidOperationException("Close action must be set on each menu.");
            }

            var control = new CompositeControl
            {
                background ?? new ComplexBox
                {
                    Fill = Constants.UI.Colors.Get(BackgroundColor.Default) * 0.8f,
                    Edge = Edge.Outer(1, Constants.UI.Colors.Get(BackgroundColor.MenuOutline)),
                    OuterGlow = (Constants.UI.Menu.ShadowWidth, Constants.UI.Menu.ShadowColor),
                },
            };
            var layout = control.BuildLayout()
                .ForContentBox()
                .DockFixedSizeToBottom(buttonFor(closeAction), Constants.UI.Button.Height)
                .ClearSpaceBottom(Constants.UI.Button.Height + Constants.UI.LayoutMargin);

            // Cast to enumerable so the Reverse cannot be mistaken for the list.Reverse method.
            var actionEnumerable = (IEnumerable<ButtonAction>)menuActions;
            foreach (var action in actionEnumerable.Reverse())
            {
                layout.DockFixedSizeToBottom(buttonFor(action), Constants.UI.Button.Height);
            }

            return control;

            Button buttonFor(ButtonAction action)
            {
                return ButtonFactories
                    .Button(b => b
                        .WithLabel(action.Label)
                        .WithOnClick(action.OnClick)
                        .WithShadow()
                        .WithAnimations(animations)
                    ).BindIsEnabled(action.IsEnabled);
            }
        }
    }
}
