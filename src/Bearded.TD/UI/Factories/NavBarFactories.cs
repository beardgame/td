using System;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories
{
    static class NavBarFactories
    {
        public static LayoutFactories.LayoutBuilder AddNavBar(
            this LayoutFactories.LayoutBuilder layout, Action<Builder> builderFunc)
        {
            var builder = new Builder();
            builderFunc(builder);
            layout.DockFixedSizeToBottom(builder.Build(), Constants.UI.NavBar.Height);
            return layout;
        }

        public class Builder
        {
            private Maybe<(string, VoidEventHandler)> backAction = Maybe.Nothing;
            private Maybe<(string, VoidEventHandler)> forwardAction = Maybe.Nothing;

            public Builder WithBackButton(VoidEventHandler onBack) => WithBackButton("Back", onBack);

            public Builder WithBackButton(string label, VoidEventHandler onBack)
            {
                backAction = Maybe.Just((label, onBack));
                return this;
            }

            public Builder WithForwardButton(string label, VoidEventHandler onForward)
            {
                forwardAction = Maybe.Just((label, onForward));
                return this;
            }

            public Control Build()
            {
                var control = new CompositeControl();
                backAction.Match(tuple => control.Add(
                    ButtonFactories.Button(b => b.WithLabel(tuple.Item1).WithOnClick(tuple.Item2))
                        .Anchor(a => a
                            .Left(width: Constants.UI.Button.Width)
                            .Bottom(height: Constants.UI.Button.Height))));
                forwardAction.Match(tuple => control.Add(
                    ButtonFactories.Button(b => b.WithLabel(tuple.Item1).WithOnClick(tuple.Item2))
                        .Anchor(a => a
                            .Right(width: Constants.UI.Button.Width)
                            .Bottom(height: Constants.UI.Button.Height))));
                return control;
            }
        }
    }
}
