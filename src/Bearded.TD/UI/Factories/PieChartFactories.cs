using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

record struct PieChartSegment(float Value, Color Color)
{
    public static PieChartSegment From(float value, Color color) => new(value, color);
    public static PieChartSegment<TData> From<TData>(float value, Color color, TData data) => new(value, color, data);
}

record struct PieChartSegment<TData>(float Value, Color Color, TData Data);

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

    public sealed class Builder<T>
    {
        public Builder(IEnumerable<T> segments)
        {
            throw new NotImplementedException();
        }

        public Builder<T> WithValues(Func<T, float> value)
        {
            throw new NotImplementedException();
        }

        public Builder<T> WithColors(Func<T, Color> color)
        {
            throw new NotImplementedException();
        }

        public Builder<T> WithTooltip(Func<T, string> tooltip)
        {
            throw new NotImplementedException();
        }

        public Builder<T> WithTooltip(Func<T, Control> tooltip)
        {
            throw new NotImplementedException();
        }

        public Control Build()
        {
            throw new NotImplementedException();
        }
    }
}
