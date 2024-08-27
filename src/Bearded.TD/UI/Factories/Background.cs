using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

readonly struct Background
{
    private readonly object? value;

    private Background(object? value)
    {
        this.value = value;
    }

    public static Background From(Control control) => new(control);
    public static Background From(ShapeColor fill) => new(fill);
    public static Background From(ShapeComponents components) => new(components);
    public static Background From(IReadonlyBinding<ShapeComponents> components) => new(components);

    public Control? ToControl()
    {
        return value switch
        {
            Control c => c,
            ShapeColor fill => new ComplexBox { Components = [ Fill.With(fill) ] },
            ShapeComponents components => new ComplexBox { Components = components },
            IReadonlyBinding<ShapeComponents> componentBinding => controlFrom(componentBinding),
            _ => null,
        };
    }

    private static ComplexBox controlFrom(IReadonlyBinding<ShapeComponents> components)
    {
        var control = new ComplexBox
        {
            Components = components.Value,
        };
        components.SourceUpdated += newComponents => control.Components = newComponents;
        components.ControlUpdated += newComponents => control.Components = newComponents;
        return control;
    }
}
