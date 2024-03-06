using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Rendering.UI.Gradients;
using Bearded.TD.UI.Controls;
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
                    FillColor = Constants.UI.Colors.Get(BackgroundColor.Default) * 0.8f,
                    EdgeOuterWidth = 1,
                    EdgeColor = Constants.UI.Colors.Get(BackgroundColor.BackgroundOutline),
                    GlowOuterWidth = Constants.UI.Menu.ShadowWidth,
                    GlowOuterColor = GradientParameters.SimpleGlow(Constants.UI.Menu.ShadowColor),
                },
            };
            var layout = control.BuildLayout()
                .ForContentBox()
                .DockFixedSizeToBottom(
                    ButtonFactories
                        .Button(b => b.WithLabel(closeAction.Label).WithOnClick(closeAction.OnClick))
                        .BindIsEnabled(closeAction.IsEnabled), Constants.UI.Button.Height)
                .ClearSpaceBottom(Constants.UI.Button.Height + Constants.UI.LayoutMargin);

            // Cast to enumerable so the Reverse cannot be mistaken for the list.Reverse method.
            var actionEnumerable = (IEnumerable<ButtonAction>)menuActions;
            foreach (var action in actionEnumerable.Reverse())
            {
                layout.DockFixedSizeToBottom(
                    ButtonFactories
                        .Button(b => b.WithLabel(action.Label).WithOnClick(action.OnClick).WithShadow())
                        .BindIsEnabled(action.IsEnabled), Constants.UI.Button.Height);
            }

            return control;
        }
    }
}
