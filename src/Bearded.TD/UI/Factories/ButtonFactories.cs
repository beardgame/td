using System;
using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI.Button;
using static Bearded.TD.Constants.UI.Text;
using static Bearded.TD.Utilities.DebugAssert;

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
            private Func<string>? labelProvider;
            private VoidEventHandler? onClick;

            public Builder WithLabel(string label)
            {
                labelProvider = () => label;
                return this;
            }

            public Builder WithLabel(Func<string> labelFunc)
            {
                labelProvider = labelFunc;
                return this;
            }

            public Builder WithOnClick(VoidEventHandler onClick)
            {
                this.onClick = onClick;
                return this;
            }

            public Button Build()
            {
                State.Satisfies(labelProvider != null);

                // ReSharper disable once UseObjectOrCollectionInitializer
                var button = new Button();

                button.Add(new DynamicLabel(labelProvider!, colorProvider) { FontSize = Constants.UI.Button.FontSize });
                button.Add(new DynamicBorder(colorProvider));
                button.Add(new ButtonBackgroundEffect(() => button.IsEnabled));

                if (onClick != null)
                {
                    button.Clicked += _ => onClick();
                }
                return button;

                Color colorProvider() => button.IsEnabled ? TextColor : DisabledTextColor;
            }
        }
    }
}
