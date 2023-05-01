using System.Collections.Generic;
using Bearded.TD.Utilities;
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

    public sealed class Builder
    {
        private readonly List<BuilderFunc<ButtonFactories.Builder>> buttonBuilderFunctions = new();

        public Builder AddButton(string label, VoidEventHandler onClick, IReadonlyBinding<bool>? isActive = null)
        {
            buttonBuilderFunctions.Add(b =>
            {
                b = b.WithLabel(label).WithOnClick(onClick);
                if (isActive != null)
                {
                    b.WithActive(isActive);
                }
                return b;
            });
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
