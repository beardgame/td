using System;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Dragging;
using Bearded.TD.Utilities;
using Bearded.UI;
using Bearded.UI.Controls;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Factories;

using static SliderDirection;

[Flags]
enum SliderDirection
{
    Horizontal = 1,
    Vertical = 2,
    Both = Horizontal | Vertical,
}

enum DragCommitMode
{
    OnDraw = 0,
    OnRelease = 1,
}

sealed class SliderFactory(ButtonFactory buttons, Animations animations)
{
    private static readonly Interval zeroToOne = Interval.FromStartAndEnd(0, 1);

    public Control Create(Func<Builder, Builder> configure)
    {
        return configure(new Builder(buttons, animations)).Build();
    }

    public sealed class Builder(ButtonFactory buttons, Animations animations)
    {
        private (Binding<double> binding, Interval range)? horizontalValue;
        private (Binding<double> binding, Interval range)? verticalValue;
        private (Binding<Vector2d> binding, Frame range)? value;
        private double? stepX;
        private double? stepY;
        private Background background;
        private (Control handle, Vector2d handleSize)? handleOverride;
        private DragCommitMode commitMode;

        public Builder WithHorizontalValue(Binding<double> binding, Interval? range = null, double? step = null)
        {
            horizontalValue = (binding, range ?? zeroToOne);
            stepX = step;
            return this;
        }

        public Builder WithVerticalValue(Binding<double> binding, Interval? range = null, double? step = null)
        {
            verticalValue = (binding, range ?? zeroToOne);
            stepY = step;
            return this;
        }

        public Builder WithValue(Binding<Vector2d> binding, Frame? range, (double? X, double? Y)? step = null)
        {
            value = (binding, range ?? new Frame(zeroToOne, zeroToOne));
            stepX = step?.X;
            stepY = step?.Y;
            return this;
        }

        public Builder WithBackground(Background background)
        {
            this.background = background;
            return this;
        }

        public Builder WithCommitMode(DragCommitMode mode)
        {
            commitMode = mode;
            return this;
        }

        public Builder WithHandle(Control handle, Vector2d size)
        {
            handleOverride = (handle, size);
            return this;
        }

        public Control Build()
        {
            var (handle, handleSize) = handleOverride ?? (
                buttons.TextButton(b => b
                    .WithLabel("")
                    .MakeHexagon()
                ),
                (16, 16)
            );

            var container = new CompositeControl();

            if (background.ToControl() is { } backgroundControl)
                container.Add(backgroundControl);

            container.Add(handle);

            var (direction, valueBinding, valueRange) = (value, horizontalValue, verticalValue) switch
            {
                (var (b, r), null, null) => (Both, b, r),

                (null, { } h, { } v) => (
                    Both,
                    Binding.Combine(h.binding, v.binding, (x, y) => new Vector2d(x, y), xy => xy.X, xy => xy.Y),
                    new Frame(h.range, v.range)
                ),

                (null, var (b, r), null) => (
                    Horizontal,
                    b.Transform(x => new Vector2d(x, 0), v => v.X),
                    new Frame(r, zeroToOne)
                ),

                (null, null, var (b, r)) => (
                    Vertical,
                    b.Transform(y => new Vector2d(0, y), v => v.Y),
                    new Frame(zeroToOne, r)
                ),
                _ => throw new InvalidOperationException(
                    "Slider must have at least one valid, distinct, value binding."),
            };

            var interaction = new DragInteraction(
                direction, container, handle, handleSize,
                valueBinding, valueRange, (stepX, stepY),
                commitMode, animations
            );
            interaction.Initialise();
            handle.AddDragging(DragScope.Anywhere, interaction.HandleDrag, interaction.HandleDragEnd);
            container.AddDragging(DragScope.Anywhere, interaction.HandleDrag, interaction.HandleDragEnd);

            return container;
        }
    }

    private sealed class DragInteraction(
        SliderDirection direction,
        Control container,
        Control handle,
        Vector2d handleSize,
        Binding<Vector2d> position,
        Frame range,
        (double? X, double? Y) step,
        DragCommitMode commitMode,
        Animations animations)
    {
        private Vector2d currentValue;
        private Vector2d currentPercentage;
        private IAnimationController? animation;

        public void Initialise()
        {
            position.SourceUpdated += updateFromSource;
            currentPercentage = percentageFromValue(position.Value);
            updateFromSource(position.Value);
        }

        private void updateFromSource(Vector2d value)
        {
            currentValue = value;
            var validValue = clampValueToValid(value);
            moveHandleToValue(validValue);
        }

        public void HandleDrag(DragEvent move)
        {
            var containerFrame = container.Frame;
            var newCenterCandidate = move.MousePosition;

            var value = valueAtPositionInFrame(newCenterCandidate, containerFrame);
            var validValue = clampValueToValid(value);

            currentValue = validValue;
            moveHandleToValue(validValue);

            if (commitMode == DragCommitMode.OnDraw)
            {
                position.SetFromControl(currentValue);
            }
        }

        public void HandleDragEnd()
        {
            if (commitMode == DragCommitMode.OnRelease)
            {
                position.SetFromControl(currentValue);
            }
        }

        private Vector2d valueAtPositionInFrame(Vector2d candidatePosition, Frame containerFrame)
        {
            var normalisedPosition = candidatePosition - containerFrame.TopLeft - handleSize * 0.5;
            var normalisedContainerFrame = containerFrame.Size - handleSize;

            var percentage = normalisedPosition / normalisedContainerFrame;

            return percentage * range.Size + range.TopLeft;
        }

        private Vector2d clampValueToValid(Vector2d value)
        {
            var steppedValue = new Vector2d(
                step.X is { } stepX ? Math.Round(value.X / stepX) * stepX : value.X,
                step.Y is { } stepY ? Math.Round(value.Y / stepY) * stepY : value.Y
            );

            return new Vector2d(
                direction.HasFlag(Horizontal) ? steppedValue.X.Clamped(range.X) : 0,
                direction.HasFlag(Vertical) ? steppedValue.Y.Clamped(range.Y) : 0
            );
        }

        private void moveHandleToValue(Vector2d value)
        {
            var p = percentageFromValue(value);

            p = new Vector2d(
                direction.HasFlag(Horizontal) ? p.X : 0.5,
                direction.HasFlag(Vertical) ? p.Y : 0.5
            );

            var distance = Vector2d.Distance(currentPercentage, p);
            var animationTime = distance / (1 + distance) * 0.5;

            var animationStart = currentPercentage;
            var animationEnd = p;

            animation?.Cancel();

            if (animationStart == animationEnd)
            {
                setHandlePositionFromPercentage(currentPercentage);
                return;
            }

            animation = animations.Start(AnimationFunction.ZeroToOne(animationTime.S(), t =>
            {
                t = Interpolate.Hermite(0, 1, 1, 0, t);
                currentPercentage = Vector2d.Lerp(animationStart, animationEnd, t);
                setHandlePositionFromPercentage(currentPercentage);
            }));
        }

        private Vector2d percentageFromValue(Vector2d value)
        {
            return (value - range.TopLeft) / range.Size;
        }

        private void setHandlePositionFromPercentage(Vector2d p)
        {
            handle.Anchor(a => a
                .Left(relativePercentage: p.X, margin: -handleSize.X * p.X, width: handleSize.X)
                .Top(relativePercentage: p.Y, margin: -handleSize.Y * p.Y, height: handleSize.Y)
            );
        }
    }
}
