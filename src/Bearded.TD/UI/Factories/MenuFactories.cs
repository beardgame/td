using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI.Menu;

namespace Bearded.TD.UI.Factories;

static class MenuFactories
{
    public static Layouts.Layout AddMenu(
        this Layouts.Layout layout, BuilderFunc<Builder> builderFunc)
    {
        var builder = new Builder();
        builderFunc(builder);
        layout.DockFixedSizeToRight(builder.Build(), Width);
        return layout;
    }

    public sealed class Builder
    {
        private ButtonAction? closeAction;
        private readonly List<ButtonAction> menuActions = new();
        private Animations? animations;
        private bool blurBackground;

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

        public Builder WithBlurredBackground()
        {
            blurBackground = true;
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
                new ComplexBox { Components = DefaultBackgroundComponents }
                    .WithDecorations(new Decorations(
                        BlurredBackground: BlurredBackground.Default.If(blurBackground))
                    ),
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
