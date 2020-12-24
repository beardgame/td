using System;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI.Button;

namespace Bearded.TD.UI.Factories
{
    static class ButtonFactories
    {
        public static Button Button(string label) => Button(b => b.WithLabel(label));

        public static Button Button(Func<string> labelFunc) => Button(b => b.WithLabel(labelFunc));

        public static Button Button(BuilderFunc<Builder> builderFunc)
        {
            var builder = new Builder();
            builderFunc(builder);
            return builder.Build();
        }

        public static Layouts.IColumnLayout AddButton(
            this Layouts.IColumnLayout columnLayout, BuilderFunc<Builder> builderFunc)
        {
            return columnLayout.Add(Button(builderFunc).WrapVerticallyCentered(Height), Height + 2 * Margin);
        }

        public sealed class Builder
        {
            private Control? labelControl;
            private VoidEventHandler? onClick;

            public Builder WithLabel(string label)
            {
                labelControl = new Label(label)
                {
                    FontSize = FontSize
                };
                return this;
            }

            public Builder WithLabel(Func<string> labelFunc)
            {
                labelControl = new DynamicLabel(labelFunc)
                {
                    FontSize = FontSize
                };
                return this;
            }

            public Builder WithOnClick(VoidEventHandler onClick)
            {
                this.onClick = onClick;
                return this;
            }

            public Button Build()
            {
                DebugAssert.State.Satisfies(labelControl != null);
                var button = new Button {labelControl, new Border(), new ButtonBackgroundEffect()};
                if (onClick != null)
                {
                    button.Clicked += onClick;
                }
                return button;
            }
        }
    }
}
