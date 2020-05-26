using System;
using System.Collections.Generic;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories
{
    static class TabBarFactories
    {
        public static LayoutFactories.LayoutBuilder AddTabs(
            this LayoutFactories.LayoutBuilder layout, Action<Builder> builderFunc)
        {
            var builder = new Builder();
            builderFunc(builder);
            layout.DockFixedSizeToTop(builder.Build(), Constants.UI.NavBar.Height);
            return layout;
        }

        public class Builder
        {
            private readonly List<Action<ButtonFactories.Builder>> buttonBuilderFunctions =
                new List<Action<ButtonFactories.Builder>>();

            public Builder AddButton(string label, VoidEventHandler onClick)
            {
                buttonBuilderFunctions.Add(b => b.WithLabel(label).WithOnClick(onClick));
                return this;
            }

            public Control Build()
            {
                var control = new CompositeControl();
                for (var i = 0; i < buttonBuilderFunctions.Count; i++)
                {
                    var leftMargin = (i + 1) * Constants.UI.BoxPadding + i * Constants.UI.Button.Width;
                    control.Add(ButtonFactories.Button(buttonBuilderFunctions[i]).Anchor(a => a
                        .Left(leftMargin, Constants.UI.Button.Width)
                        .Top(Constants.UI.BoxPadding, Constants.UI.Button.Height)));
                }
                return control;
            }
        }
    }
}
