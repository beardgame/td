using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI.Button;

namespace Bearded.TD.UI.Factories;

static partial class ButtonFactories
{
    public static Button Button(string label) => Button(b => b.WithLabel(label));

    public static Button Button(BuilderFunc<TextButtonBuilder> builderFunc) =>
        button(new TextButtonBuilder(), builderFunc);

    public static Button StandaloneIconButton(BuilderFunc<IconButtonBuilder> builderFunc) =>
        button(IconButtonBuilder.ForStandaloneButton(), builderFunc);

    private static Button button<T>(T builder, BuilderFunc<T> builderFunc) where T : Builder<T>
    {
        builderFunc(builder);
        return builder.Build();
    }

    public static Layouts.IColumnLayout AddButton(
        this Layouts.IColumnLayout columnLayout, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return columnLayout.Add(Button(builderFunc).WrapVerticallyCentered(Height), Height + 2 * Margin);
    }

    public static Layouts.IColumnLayout AddCenteredButton(
        this Layouts.IColumnLayout columnLayout, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return columnLayout.Add(
            Button(builderFunc).WrapCentered(Width, Height),
            Height + 2 * Margin);
    }

    public static Layouts.IRowLayout AddButtonLeft(
        this Layouts.IRowLayout rowLayout, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return rowLayout.AddLeft(Button(builderFunc).WrapCentered(Width, Height), Width + 2 * Margin);
    }

    public static Layouts.IRowLayout AddButtonRight(
        this Layouts.IRowLayout rowLayout, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return rowLayout.AddRight(Button(builderFunc).WrapCentered(Width, Height), Width + 2 * Margin);
    }

    public static Layouts.IRowLayout AddSquareButtonRight(
        this Layouts.IRowLayout rowLayout, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return rowLayout.AddRight(Button(builderFunc).WrapCentered(Height, Height), Height + 2 * Margin);
    }
}
