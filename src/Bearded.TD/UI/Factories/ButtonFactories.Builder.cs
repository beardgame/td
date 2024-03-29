using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Factories;

static partial class ButtonFactories
{
    public abstract class Builder<T> where T : Builder<T>
    {
        private enum Shape
        {
            Rectangle = 0,
            Circle,
            Hexagon,
        }

        private (IReadonlyBinding<double> Progress, Color? Color)? progressBar;
        private (TooltipFactory Factory, TooltipDefinition Definition)? tooltip;
        private GenericEventHandler<Button.ClickEventArgs>? onClick;
        private IReadonlyBinding<bool> isEnabled = Binding.Constant(true);
        private IReadonlyBinding<bool> isActive = Binding.Constant(false);
        private IReadonlyBinding<bool> isError = Binding.Constant(false);
        private Shape shape;
        private Shadow? shadow;
        private Animations? animations;

        protected abstract T This { get; }

        public T WithAnimations(Animations? animations)
        {
            this.animations = animations;
            return This;
        }

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

        public T WithShadow(Shadow? shadow = null)
        {
            this.shadow = shadow ?? Constants.UI.Button.DefaultShadow;
            return This;
        }

        public T MakeDisabled()
        {
            isEnabled = Binding.Constant(false);
            return This;
        }

        public T MakeCircle()
        {
            shape = Shape.Circle;
            return This;
        }
        public T MakeHexagon()
        {
            shape = Shape.Hexagon;
            return This;
        }

        public Button Build()
        {
            Validate();

            // ReSharper disable once UseObjectOrCollectionInitializer
            var button = new Button();
            var contentColor = Binding.Combine(isEnabled, isError, (enabled, error) =>
            {
                if (error)
                {
                    return Constants.UI.Text.ErrorTextColor;
                }

                return enabled ? Constants.UI.Text.TextColor : Constants.UI.Text.DisabledTextColor;
            });

            AddContent(button, contentColor);

            var shape = this.shape switch
            {
                Shape.Rectangle => (ComplexShapeControl)new ComplexBox { CornerRadius = 2 },
                Shape.Circle => new ComplexCircle(),
                Shape.Hexagon => new ComplexHexagon { CornerRadius = 2 },
                _ => throw new ArgumentOutOfRangeException(),
            };
            shape.Edge = Edge.Outer(1, contentColor.Value);

            if (shadow is { } s)
            {
                button.Add(new DropShadow
                {
                    SourceControl = shape,
                    Shadow = s,
                });
            }

            button.Add(shape);
            contentColor.SourceUpdated += c => shape.Edge = shape.Edge with { Color = c };

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

            var mouseState = new MouseStateObserver(button);

            isEnabled.SourceUpdated += enabled => button.IsEnabled = enabled;
            button.IsEnabled = isEnabled.Value;

            IAnimationController? backgroundAnimation = null;
            isActive.SourceUpdated += _ => updateColor();
            isActive.ControlUpdated += _ => updateColor();
            isEnabled.SourceUpdated += _ => updateColor();
            isEnabled.ControlUpdated += _ => updateColor();
            mouseState.StateChanged += () => updateColor();
            updateColor(true);

            if (onClick != null)
            {
                button.Clicked += args => onClick(args);
            }


            return button;

            void updateColor(bool skipAnimation = false)
            {
                var color = (button, mouseState) switch
                {
                    ({ IsEnabled: false }, _) => BackgroundColor.InactiveElement,
                    (_, { MouseIsDown: true }) => BackgroundColor.ActiveElement,
                    (_, { MouseIsOver: true }) => BackgroundColor.Hover,
                    _ when isActive.Value => BackgroundColor.ActiveElement,
                    _ => BackgroundColor.Element,
                };

                if (skipAnimation || animations == null)
                {
                    shape.Fill = Constants.UI.Colors.Get(color);
                    return;
                }
                backgroundAnimation?.Cancel();
                backgroundAnimation = animations.Start(Constants.UI.Button.BackgroundColorAnimation(shape, color));
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
                control.Add(costLabel.Anchor(a =>
                    a.Bottom(margin: Constants.UI.Button.Margin).Top(relativePercentage: .6)));
            }
        }
    }

    public sealed class IconButtonBuilder : Builder<IconButtonBuilder>
    {
        private IReadonlyBinding<ModAwareSpriteId>? icon;
        private float scale = 1;

        protected override IconButtonBuilder This => this;

        public static IconButtonBuilder ForStandaloneButton() => new();

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

        public IconButtonBuilder WithIconScale(float scale)
        {
            this.scale = scale;
            return this;
        }

        protected override void Validate()
        {
            DebugAssert.State.Satisfies(icon != null);
        }

        protected override void AddContent(IControlParent control, IReadonlyBinding<Color> color)
        {
            var iconControl = new Sprite { SpriteId = icon!.Value, Color = color.Value };
            var l = iconControl.Layout;
            iconControl.Layout = l with { Scale = l.Scale * scale };
            control.Add(iconControl);

            icon.SourceUpdated += id => iconControl.SpriteId = id;
            color.SourceUpdated += c => iconControl.Color = c;

            iconControl.BindIsVisible(icon.Transform(id => id.SpriteSet.IsValid && !string.IsNullOrWhiteSpace(id.Id)));
        }
    }
}
