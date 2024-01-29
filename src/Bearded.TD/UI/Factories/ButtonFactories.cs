using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI.Button;
using static Bearded.TD.Constants.UI.Text;
using static Bearded.TD.UI.Factories.TooltipFactories;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.UI.Factories;

static class ButtonFactories
{
    public static Button Button(string label) => Button(b => b.WithLabel(label));

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

    public static Layouts.IColumnLayout AddCenteredButton(
        this Layouts.IColumnLayout columnLayout, BuilderFunc<Builder> builderFunc)
    {
        return columnLayout.Add(
            Button(builderFunc).WrapCentered(Width, Height),
            Height + 2 * Margin);
    }

    public static Layouts.IRowLayout AddButtonLeft(
        this Layouts.IRowLayout rowLayout, BuilderFunc<Builder> builderFunc)
    {
        return rowLayout.AddLeft(Button(builderFunc).WrapCentered(Width, Height), Width + 2 * Margin);
    }

    public static Layouts.IRowLayout AddButtonRight(
        this Layouts.IRowLayout rowLayout, BuilderFunc<Builder> builderFunc)
    {
        return rowLayout.AddRight(Button(builderFunc).WrapCentered(Width, Height), Width + 2 * Margin);
    }

    public sealed class Builder
    {
        private Func<string>? labelProvider;
        private (IReadonlyBinding<int> CostAmount, Color Color)? cost;
        private (IReadonlyBinding<double> Progress, Color? Color)? progressBar;
        private (TooltipFactory Factory, TooltipDefinition Definition)? tooltip;
        private GenericEventHandler<Button.ClickEventArgs>? onClick;
        private IReadonlyBinding<bool>? isEnabled;
        private IReadonlyBinding<bool>? isActive;
        private IReadonlyBinding<bool>? isError;
        private bool isDisabled;

        public Builder WithLabel(string label)
        {
            labelProvider = () => label;
            return this;
        }

        public Builder WithLabel(IReadonlyBinding<string> labelBinding)
        {
            labelProvider = () => labelBinding.Value;
            return this;
        }

        [Obsolete("Use a binding instead")]
        public Builder WithLabel(Func<string> labelFunc)
        {
            labelProvider = labelFunc;
            return this;
        }

        public Builder WithResourceCost(ResourceAmount amount)
        {
            cost = (new Binding<int>(amount.NumericValue), Constants.Game.GameUI.ResourcesColor);
            return this;
        }

        public Builder WithResourceCost(IReadonlyBinding<ResourceAmount> amount)
        {
            cost = (amount.Transform(r => r.NumericValue), Constants.Game.GameUI.ResourcesColor);
            return this;
        }

        public Builder WithTechCost(int amount)
        {
            cost = (new Binding<int>(amount), Constants.Game.GameUI.TechPointsColor);
            return this;
        }

        public Builder WithProgressBar(Binding<double> progress, Color? color = null)
        {
            progressBar = (progress, color);
            return this;
        }

        public Builder WithTooltip(TooltipFactory factory, string text) => WithTooltip(factory, SimpleTooltip(text));

        public Builder WithTooltip(TooltipFactory factory, ICollection<string> text) =>
            WithTooltip(factory, SimpleTooltip(text));

        public Builder WithTooltip(TooltipFactory factory, TooltipDefinition definition)
        {
            tooltip = (factory, definition);
            return this;
        }

        public Builder WithOnClick(VoidEventHandler onClick)
        {
            this.onClick = _ => onClick();
            return this;
        }

        public Builder WithOnClick(GenericEventHandler<Button.ClickEventArgs> onClick)
        {
            this.onClick = onClick;
            return this;
        }

        public Builder WithEnabled(IReadonlyBinding<bool> isEnabled)
        {
            this.isEnabled = isEnabled;
            return this;
        }

        public Builder WithActive(IReadonlyBinding<bool> isActive)
        {
            this.isActive = isActive;
            return this;
        }

        public Builder WithError(IReadonlyBinding<bool> isError)
        {
            this.isError = isError;
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
                var costLabel = new Label
                {
                    Text = cost.Value.CostAmount.Value.ToString(),
                    Color = cost.Value.Color,
                    FontSize = CostFontSize,
                };
                cost.Value.CostAmount.SourceUpdated += newCost => costLabel.Text = newCost.ToString();

                button.Add(costLabel.Anchor(a => a.Bottom(margin: Margin).Top(relativePercentage: .6)));
            }

            button.Add(new DynamicBorder(colorProvider));

            if (progressBar.HasValue)
            {
                var color = progressBar.Value.Color ?? Color.White * .25f;
                button.Add(ProgressBarFactories.BareProgressBar(progressBar.Value.Progress, color));
            }

            if (tooltip.HasValue)
            {
                button.Add(
                    new TooltipTarget(tooltip.Value.Factory, tooltip.Value.Definition, TooltipAnchor.Direction.Right));
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
                button.Clicked += args => onClick(args);
            }
            return button;

            Color colorProvider()
            {
                if (isError?.Value ?? false)
                {
                    return ErrorTextColor;
                }
                return button.IsEnabled ? TextColor : DisabledTextColor;
            }
        }
    }
}
