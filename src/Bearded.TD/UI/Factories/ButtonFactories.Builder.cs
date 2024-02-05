using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
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

        public T WithTooltip(TooltipFactory factory, string text) =>
            WithTooltip(factory, TooltipFactories.SimpleTooltip(text));

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

            var border = new Border();
            button.Add(border);
            color.SourceUpdated += c => border.Color = c;
            border.Color = color.Value;

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

    public sealed class IconButtonBuilder : Builder<IconButtonBuilder>
    {
        private ButtonSize size;
        private IReadonlyBinding<ModAwareSpriteId>? icon;

        protected override IconButtonBuilder This => this;

        public static IconButtonBuilder ForInlineButton() => new(inlineButtonSize);
        public static IconButtonBuilder ForStandaloneButton() => new(standaloneButtonSize);

        private IconButtonBuilder(ButtonSize size)
        {
            this.size = size;
        }

        public IconButtonBuilder WithIcon(ModAwareSpriteId icon)
        {
            this.icon = Binding.Constant(icon);
            return this;
        }

        public IconButtonBuilder WithIcon(IReadonlyBinding<ModAwareSpriteId> icon)
        {
            this.icon = icon;
            return this;
        }

        public IconButtonBuilder WithCustomSize(double buttonSize)
        {
            size = new ButtonSize(buttonSize, buttonSize);
            return this;
        }

        protected override void Validate()
        {
            DebugAssert.State.Satisfies(icon != null);
        }

        protected override void AddContent(IControlParent control, IReadonlyBinding<Color> color)
        {
            var iconControl = new Sprite { SpriteId = icon!.Value, Color = color.Value, Size = size.SpriteSize };
            control.Add(iconControl);

            icon.SourceUpdated += id => iconControl.SpriteId = id;
            color.SourceUpdated += c => iconControl.Color = c;

            iconControl.BindIsVisible(icon.Transform(id => id.SpriteSet.IsValid && !string.IsNullOrWhiteSpace(id.Id)));
        }

        public Button Build(out double buttonSize)
        {
            buttonSize = size.Size;
            return Build();
        }

        private readonly record struct ButtonSize(double Size, double SpriteSize);

        private static readonly ButtonSize inlineButtonSize =
            new(Constants.UI.Text.LineHeight, Constants.UI.Text.LineHeight);

        private static readonly ButtonSize standaloneButtonSize =
            new(Constants.UI.Button.SquareButtonSize, Constants.UI.Button.SquareButtonSize);
    }
}
