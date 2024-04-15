using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Controls;
using Bearded.Utilities.Geometry;

namespace Bearded.TD.UI.Factories;

record struct PieChartSegment(float Value, Color Color)
{
    public static PieChartSegment From(float value, Color color) => new(value, color);
    public static PieChartSegment<TData> From<TData>(float value, Color color, TData data) => new(value, color, data);
}

record struct PieChartSegment<TData>(float Value, Color Color, TData Data);

enum Sign
{
    Positive,
    Negative,
}

static class PieChartFactories
{
    public static Control StaticPieChart(IEnumerable<PieChartSegment> segments)
    {
        var builder = new Builder<PieChartSegment>(segments)
            .WithValues(s => s.Value)
            .WithColors(s => s.Color);
        return builder.Build();
    }

    public static Control StaticPieChart<T>(IEnumerable<T> segments, Action<Builder<T>> configureBuilder)
    {
        var builder = new Builder<T>(segments);
        configureBuilder(builder);
        return builder.Build();
    }

    public static Control StaticPieChart<TData>(
        IEnumerable<PieChartSegment<TData>> segments,
        Action<Builder<PieChartSegment<TData>>> configureBuilder
    )
    {
        var builder = new Builder<PieChartSegment<TData>>(segments)
            .WithValues(s => s.Value)
            .WithColors(s => s.Color);
        configureBuilder(builder);
        return builder.Build();
    }

    public sealed class Builder<T>(IEnumerable<T> segments)
    {
        private Func<T, float>? getValue;
        private Func<T, Color>? getColor;
        private Func<T, string>? getTooltipString;
        private Func<T, Control>? getTooltipControl;
        private Direction2 origin;
        private Sign sign;
        private Shadow? shadow;
        private ShapeComponents additionalComponents;

        public Builder<T> WithValues(Func<T, float> value)
        {
            getValue = value;
            return this;
        }

        public Builder<T> WithColors(Func<T, Color> color)
        {
            getColor = color;
            return this;
        }

        public Builder<T> WithTooltip(Func<T, string> tooltip)
        {
            throw new NotImplementedException();
            getTooltipString = tooltip;
            return this;
        }

        public Builder<T> WithTooltip(Func<T, Control> tooltip)
        {
            throw new NotImplementedException();
            getTooltipControl = tooltip;
            return this;
        }

        public Builder<T> WithOriginAngle(Direction2 direction)
        {
            origin = direction;
            return this;
        }

        public Builder<T> WithDirection(Sign direction)
        {
            sign = direction;
            return this;
        }

        public Builder<T> WithShadow(Shadow shadow)
        {
            this.shadow = shadow;
            return this;
        }

        public Builder<T> WithAdditionalComponents(ShapeComponents components)
        {
            additionalComponents = components;
            return this;
        }

        public Control Build()
        {
            if (getValue == null)
            {
                throw new InvalidOperationException("Value accessor must be set.");
            }

            if (getColor == null)
            {
                throw new InvalidOperationException("Color accessor must be set.");
            }

            if (!segments.Any())
                return new CompositeControl();

            var segmentList = segments as ICollection<T> ?? segments.ToList();
            var totalValue = segmentList.Sum(getValue);

            var gradient = ImmutableArray.CreateBuilder<GradientStop>(segmentList.Count * 2);

            var valueSoFar = 0f;

            foreach (var segment in segmentList)
            {
                var value = getValue(segment);
                var color = getColor(segment);

                gradient.Add(new GradientStop(valueSoFar / totalValue, color));
                valueSoFar += value;
                gradient.Add(new GradientStop(valueSoFar / totalValue, color));
            }

            var fullCircle = Angle.FromRadians(sign == Sign.Positive ? 2 * MathF.PI : -2 * MathF.PI);

            var pie = new ComplexCircle
            {
                Components = [
                    Fill.With(ShapeColor.From(
                        gradient.MoveToImmutable(),
                        GradientDefinition.ArcAroundPoint(AnchorPoint.FrameCenter, origin, fullCircle)
                    )),
                    ..additionalComponents,
                ],
            };

            return shadow switch
            {
                null => pie,
                { } s => new CompositeControl { pie.WithDropShadow(s) },
            };
        }
    }
}
