using System;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Factories;

static class ButtonFactoryExtensions
{
    public static T WithProgressBar<T>
    (
        this T builder,
        IReadonlyBinding<double> progress,
        int gradientStops,
        Action<double, GradientStop[]> updateGradient,
        Func<GradientStop[], ShapeComponent> createComponentOnce
    )
        where T : ButtonFactory.Builder<T>
    {
        var stops = new GradientStop[gradientStops];
        var component = createComponentOnce(stops);

        builder.WithAdditionalShapeComponents([component]);
        progress.SourceUpdated += p => updateGradient(p, stops);

        return builder;
    }

    public static T WithProgressBar<T>
    (
        this T builder,
        IReadonlyBinding<double> progress,
        int gradientStops,
        Func<double, GradientStop[], ShapeComponent> createComponent
    )
        where T : ButtonFactory.Builder<T>
    {
        var stops = new GradientStop[gradientStops];

        WithProgressBar(builder, progress, p => createComponent(p, stops));

        return builder;
    }

    public static T WithProgressBar<T>
    (
        this T builder,
        IReadonlyBinding<double> progress,
        Func<double, ShapeComponent> update
    )
        where T : ButtonFactory.Builder<T>
    {
        builder.WithAdditionalMutableShapeComponent(out var setComponent);
        setComponent(update(progress.Value));

        progress.SourceUpdated += p => setComponent(update(p));

        return builder;
    }
}
