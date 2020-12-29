using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories
{
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

            public Builder WithBackButton(string label, VoidEventHandler onBack, Binding<bool>? enabledBinding = null)
            {
                backAction = new ButtonAction(label, onBack, enabledBinding);
                return this;
            }

            public Builder WithForwardButton(
                string label, VoidEventHandler onForward, Binding<bool>? enabledBinding = null)
            {
                forwardAction = new ButtonAction(label, onForward, enabledBinding);
                return this;
            }

            public Control Build()
            {
                var control = new CompositeControl();
                if (backAction != null)
                {
                    var button = ButtonFactories
                        .Button(b => b.WithLabel(backAction.Label).WithOnClick(backAction.OnClick))
                        .Anchor(a => a
                            .Left(width: Constants.UI.Button.Width)
                            .Bottom(height: Constants.UI.Button.Height));
                    if (backAction.EnabledBinding != null)
                    {
                        button.IsEnabled = backAction.EnabledBinding.Value;
                        backAction.EnabledBinding.SourceUpdated += enabled => button.IsEnabled = enabled;
                    }
                    control.Add(button);
                }
                if (forwardAction != null)
                {
                    var button = ButtonFactories
                        .Button(b => b.WithLabel(forwardAction.Label).WithOnClick(forwardAction.OnClick))
                        .Anchor(a => a
                            .Right(width: Constants.UI.Button.Width)
                            .Bottom(height: Constants.UI.Button.Height));
                    if (forwardAction.EnabledBinding != null)
                    {
                        button.IsEnabled = forwardAction.EnabledBinding.Value;
                        forwardAction.EnabledBinding.SourceUpdated += enabled => button.IsEnabled = enabled;
                    }
                    control.Add(button);
                }

                return control;
            }

            private sealed record ButtonAction
            {
                public string Label { get; }
                public VoidEventHandler OnClick { get; }
                public Binding<bool>? EnabledBinding { get; }

                public ButtonAction(string label, VoidEventHandler onClick, Binding<bool>? enabledBinding)
                {
                    Label = label;
                    OnClick = onClick;
                    EnabledBinding = enabledBinding;
                }
            }
        }
    }
}
