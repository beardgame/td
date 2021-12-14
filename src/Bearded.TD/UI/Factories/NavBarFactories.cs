using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories;

static class NavBarFactories
{
    public static Layouts.Layout AddNavBar(
        this Layouts.Layout layout, BuilderFunc<Builder> builderFunc)
    {
        var builder = new Builder();
        builderFunc(builder);
        layout.DockFixedSizeToBottom(builder.Build(), Constants.UI.NavBar.Height);
        return layout;
    }

    public sealed class Builder
    {
        private ButtonAction? backAction;
        private ButtonAction? forwardAction;

        public Builder WithBackButton(VoidEventHandler onBack) => WithBackButton("Back", onBack);

        public Builder WithBackButton(string label, VoidEventHandler onBack, Binding<bool>? isEnabled = null)
        {
            backAction = new ButtonAction(label, onBack, isEnabled);
            return this;
        }

        public Builder WithForwardButton(
            string label, VoidEventHandler onForward, Binding<bool>? isEnabled = null)
        {
            forwardAction = new ButtonAction(label, onForward, isEnabled);
            return this;
        }

        public Control Build()
        {
            var control = new CompositeControl();
            if (backAction != null)
            {
                control.Add(ButtonFactories
                    .Button(b => b.WithLabel(backAction.Label).WithOnClick(backAction.OnClick))
                    .Anchor(a => a
                        .Left(width: Constants.UI.Button.Width)
                        .Bottom(height: Constants.UI.Button.Height))
                    .BindIsEnabled(backAction.IsEnabled));
            }
            if (forwardAction != null)
            {
                control.Add(ButtonFactories
                    .Button(b => b.WithLabel(forwardAction.Label).WithOnClick(forwardAction.OnClick))
                    .Anchor(a => a
                        .Right(width: Constants.UI.Button.Width)
                        .Bottom(height: Constants.UI.Button.Height))
                    .BindIsEnabled(forwardAction.IsEnabled));
            }

            return control;
        }
    }
}