using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI.Menu;

namespace Bearded.TD.UI.Factories;

static class MenuFactories
{
    public static Layouts.Layout AddMenu(
        this Layouts.Layout layout, UIFactories factories, BuilderFunc<Builder> builderFunc)
    {
        var menu = factories.Menu(builderFunc);
        layout.DockFixedSizeToRight(menu, Width);
        return layout;
    }

    public static Control Menu(this UIFactories factories, BuilderFunc<Builder> f)
    {
        var builder = new Builder(factories);
        f(builder);
        return builder.Build();
    }

    public sealed class Builder(UIFactories factories)
    {
        private ButtonAction? closeAction;
        private readonly List<ButtonAction> menuActions = new();
        private bool blurBackground;

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
                return factories
                    .Button(b => b
                        .WithLabel(action.Label)
                        .WithOnClick(action.OnClick)
                        .WithShadow()
                    ).BindIsEnabled(action.IsEnabled);
            }
        }
    }
}
