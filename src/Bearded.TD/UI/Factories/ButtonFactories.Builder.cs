using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories;

static partial class ButtonFactories
{
    public abstract class Builder<T> where T : Builder<T>
    {
        private (IReadonlyBinding<double> Progress, Color? Color)? progressBar;
        private (TooltipFactory Factory, TooltipDefinition Definition)? tooltip;
        private GenericEventHandler<Button.ClickEventArgs>? onClick;
        private IReadonlyBinding<bool> isEnabled = Binding.Constant(true);
        private IReadonlyBinding<bool> isActive = Binding.Constant(false);
        private IReadonlyBinding<bool> isError = Binding.Constant(false);

        protected abstract T This { get; }

        public T WithProgressBar(Binding<double> progress, Color? color = null)
        {
            progressBar = (progress, color);
            return This;
        }

        public T WithTooltip(TooltipFactory factory, string text) => WithTooltip(factory, TooltipFactories.SimpleTooltip(text));

        public T WithTooltip(TooltipFactory factory, ICollection<string> text) =>
            WithTooltip(factory, TooltipFactories.SimpleTooltip(text));

        public T WithTooltip(TooltipFactory factory, TooltipDefinition definition)
        {
            tooltip = (factory, definition);
            return This;
        }

        public T WithOnClick(VoidEventHandler onClick)
        {
            this.onClick = _ => onClick();
            return This;
        }

        public T WithOnClick(GenericEventHandler<Button.ClickEventArgs> onClick)
        {
            this.onClick = onClick;
            return This;
        }

        public T WithEnabled(IReadonlyBinding<bool> isEnabled)
        {
            this.isEnabled = isEnabled;
            return This;
        }

        public T WithActive(IReadonlyBinding<bool> isActive)
        {
            this.isActive = isActive;
            return This;
        }

        public T WithError(IReadonlyBinding<bool> isError)
        {
            this.isError = isError;
            return This;
        }

        public T MakeDisabled()
        {
            isEnabled = Binding.Constant(false);
            return This;
        }

        public Button Build()
        {
            Validate();

            // ReSharper disable once UseObjectOrCollectionInitializer
            var button = new Button();
            var color = Binding.Combine(isEnabled, isError, (enabled, error) =>
            {
                if (error)
                {
                    return Constants.UI.Text.ErrorTextColor;
                }
                return enabled ? Constants.UI.Text.TextColor : Constants.UI.Text.DisabledTextColor;
            });

            AddContent(button, color);
            button.Add(new DynamicBorder(colorProvider));

            if (progressBar.HasValue)
            {
                var progressColor = progressBar.Value.Color ?? Color.White * .25f;
                button.Add(ProgressBarFactories.BareProgressBar(progressBar.Value.Progress, progressColor));
            }

            if (tooltip.HasValue)
            {
                button.Add(
                    new TooltipTarget(tooltip.Value.Factory, tooltip.Value.Definition, TooltipAnchor.Direction.Right));
            }

            var bg = new BackgroundBox();
            button.Add(bg);
            isActive.SourceUpdated += updateColor;
            isActive.ControlUpdated += updateColor;
            updateColor(isActive.Value);
            void updateColor(bool active) => bg.Color = active ? Constants.UI.Button.ActiveColor : Color.Transparent;

            button.Add(new ButtonBackgroundEffect(() =>
                button.IsEnabled && (progressBar?.Progress.Value ?? 0) == 0 && !isActive.Value));

            isEnabled.SourceUpdated += enabled => button.IsEnabled = enabled;
            button.IsEnabled = isEnabled.Value;

            if (onClick != null)
            {
                button.Clicked += args => onClick(args);
            }
            return button;

            Color colorProvider()
            {
                if (isError?.Value ?? false)
                {
                    return Constants.UI.Text.ErrorTextColor;
                }
                return button.IsEnabled ? Constants.UI.Text.TextColor : Constants.UI.Text.DisabledTextColor;
            }
        }

        protected abstract void Validate();
        protected abstract void AddContent(IControlParent control, IReadonlyBinding<Color> color);
    }

    public sealed class TextButtonBuilder : Builder<TextButtonBuilder>
    {
        private IReadonlyBinding<string>? label;
        private IReadonlyBinding<string>? cost;

        protected override TextButtonBuilder This => this;

        public TextButtonBuilder WithLabel(string label)
        {
            this.label = Binding.Constant(label);
            return this;
        }

        public TextButtonBuilder WithLabel(IReadonlyBinding<string> labelBinding)
        {
            label = labelBinding;
            return this;
        }

        public TextButtonBuilder WithResourceCost(ResourceAmount amount)
        {
            cost = Binding.Constant(amount.NumericValue.ToString());
            return this;
        }

        public TextButtonBuilder WithResourceCost(IReadonlyBinding<ResourceAmount> amount)
        {
            cost = amount.Transform(r => r.NumericValue.ToString());
            return this;
        }

        protected override void Validate()
        {
            DebugAssert.State.Satisfies(label != null);
        }

        protected override void AddContent(IControlParent control, IReadonlyBinding<Color> color)
        {
            var labelControl = TextFactories.Label(label!, color: color);
            labelControl.FontSize = Constants.UI.Button.FontSize;
            control.Add(labelControl);

            if (cost != null)
            {
                labelControl.Anchor(a => a.Top(margin: Constants.UI.Button.Margin).Bottom(relativePercentage: .4));
                var costLabel = TextFactories.Label(cost!);
                costLabel.FontSize = Constants.UI.Button.CostFontSize;
                control.Add(costLabel.Anchor(a => a.Bottom(margin: Constants.UI.Button.Margin).Top(relativePercentage: .6)));
            }
        }
    }
}
