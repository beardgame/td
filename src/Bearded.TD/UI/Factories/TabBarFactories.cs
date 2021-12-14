using System.Collections.Generic;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories;

static class TabBarFactories
{
    public static Layouts.Layout AddTabs(
        this Layouts.Layout layout, BuilderFunc<Builder> builderFunc)
    {
        var builder = new Builder();
        builderFunc(builder);
        layout.DockFixedSizeToTop(builder.Build(), Constants.UI.NavBar.Height);
        return layout;
    }

    public class Builder
    {
        private readonly List<BuilderFunc<ButtonFactories.Builder>> buttonBuilderFunctions =
            new List<BuilderFunc<ButtonFactories.Builder>>();

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
                var leftMargin = i * (Constants.UI.LayoutMargin + Constants.UI.Button.Width);
                control.Add(ButtonFactories.Button(buttonBuilderFunctions[i]).Anchor(a => a
                    .Left(leftMargin, Constants.UI.Button.Width)
                    .Top(height: Constants.UI.Button.Height)));
            }
            return control;
        }
    }
}