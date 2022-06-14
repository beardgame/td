using System;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.UI.Controls;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI.Button;
using static Bearded.TD.Constants.UI.Text;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.UI.Factories;

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

    public static Layouts.IRowLayout AddButtonLeft(
        this Layouts.IRowLayout rowLayout, BuilderFunc<Builder> builderFunc)
    {
        return rowLayout.AddLeft(Button(builderFunc).WrapHorizontallyCentered(Width), Width + 2 * Margin);
    }

    public static Layouts.IRowLayout AddButtonRight(
        this Layouts.IRowLayout rowLayout, BuilderFunc<Builder> builderFunc)
    {
        return rowLayout.AddRight(Button(builderFunc).WrapHorizontallyCentered(Width), Width + 2 * Margin);
    }

    public sealed class Builder
    {
        private Func<string>? labelProvider;
        private (int CostAmount, Color Color)? cost;
        private (Binding<double> Progress, Color? Color)? progressBar;
        private VoidEventHandler? onClick;
        private Binding<bool>? isEnabled;
        private Binding<bool>? isActive;
        private bool isDisabled;

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

        public Builder WithResourceCost(ResourceAmount amount)
        {
            cost = (amount.NumericValue, Constants.Game.GameUI.ResourcesColor);
            return this;
        }

        public Builder WithTechCost(int amount)
        {
            cost = (amount, Constants.Game.GameUI.TechPointsColor);
            return this;
        }

        public Builder WithProgressBar(Binding<double> progress, Color? color = null)
        {
            progressBar = (progress, color);
            return this;
        }

        public Builder WithOnClick(VoidEventHandler onClick)
        {
            this.onClick = onClick;
            return this;
        }

        public Builder WithEnabled(Binding<bool> isEnabled)
        {
            this.isEnabled = isEnabled;
            return this;
        }

        public Builder WithActive(Binding<bool> isActive)
        {
            this.isActive = isActive;
            return this;
        }

        public Builder MakeDisabled()
        {
            isDisabled = true;
            return this;
        }

        public Button Build()
        {
            State.Satisfies(labelProvider != null);

            // ReSharper disable once UseObjectOrCollectionInitializer
            var button = new Button {IsEnabled = !isDisabled};

            var label = new DynamicLabel(labelProvider!, colorProvider) {FontSize = Constants.UI.Button.FontSize};
            button.Add(label);

            if (cost.HasValue)
            {
                label.Anchor(a => a.Top(margin: Margin).Bottom(relativePercentage: .4));
                button.Add(new Label(cost.Value.CostAmount.ToString())
                {
                    Color = cost.Value.Color,
                    FontSize = CostFontSize,
                }.Anchor(a => a.Bottom(margin: Margin).Top(relativePercentage: .6)));
            }

            button.Add(new DynamicBorder(colorProvider));

            if (progressBar.HasValue)
            {
                var color = progressBar.Value.Color ?? Color.White * .25f;
                button.Add(ProgressBarFactories.BareProgressBar(progressBar.Value.Progress, color));
            }
            if (isActive != null)
            {
                var bg = new BackgroundBox();
                button.Add(bg);
                isActive.SourceUpdated += updateColor;
                isActive.ControlUpdated += updateColor;
                updateColor(isActive.Value);

                void updateColor(bool active) => bg.Color = active ? ActiveColor : Color.Transparent;
            }
            button.Add(new ButtonBackgroundEffect(() =>
                button.IsEnabled && (progressBar?.Progress.Value ?? 0) == 0 && (!isActive?.Value ?? true)));

            if (isEnabled != null)
            {
                isEnabled.SourceUpdated += enabled => button.IsEnabled = enabled;
                button.IsEnabled = isEnabled.Value;
            }

            if (onClick != null)
            {
                button.Clicked += _ => onClick();
            }
            return button;

            Color colorProvider() => button.IsEnabled ? TextColor : DisabledTextColor;
        }
    }
}
