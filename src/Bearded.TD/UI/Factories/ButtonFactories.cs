using Bearded.TD.Content.Mods;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI.Button;
using static Bearded.TD.UI.Factories.ButtonFactory;

namespace Bearded.TD.UI.Factories;

static class ButtonFactories
{
    public static Button Button(this UIFactories factories, string label) => Button(factories, b => b.WithLabel(label));

    public static Button Button(this UIFactories factories, BuilderFunc<TextButtonBuilder> builderFunc) =>
        factories.ButtonFactory.TextButton(builderFunc);

    public static Button StandaloneIconButton(this UIFactories factories, ModAwareSpriteId icon) =>
        StandaloneIconButton(factories, b => b.WithIcon(icon));

    public static Button StandaloneIconButton(this UIFactories factories, BuilderFunc<IconButtonBuilder> builderFunc) =>
        factories.ButtonFactory.IconButton(builderFunc);

    public static Layouts.IColumnLayout AddButton(
        this Layouts.IColumnLayout columnLayout, UIFactories factories, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return columnLayout.Add(factories.Button(builderFunc).WrapVerticallyCentered(Height), Height + 2 * Margin);
    }

    public static Layouts.IColumnLayout AddLeftButton(
        this Layouts.IColumnLayout columnLayout, UIFactories factories, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return columnLayout.Add(factories.Button(builderFunc).WrapAligned(Width, Height, 0, 0.5), Height + 2 * Margin);
    }

    public static Layouts.IColumnLayout AddCenteredButton(
        this Layouts.IColumnLayout columnLayout, UIFactories factories, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return columnLayout.Add(factories.Button(builderFunc).WrapCentered(Width, Height), Height + 2 * Margin);
    }

    public static Layouts.IRowLayout AddButtonLeft(
        this Layouts.IRowLayout rowLayout, UIFactories factories, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return rowLayout.AddLeft(factories.Button(builderFunc).WrapCentered(Width, Height), Width + 2 * Margin);
    }

    public static Layouts.IRowLayout AddButtonRight(
        this Layouts.IRowLayout rowLayout, UIFactories factories, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return rowLayout.AddRight(factories.Button(builderFunc).WrapCentered(Width, Height), Width + 2 * Margin);
    }

    public static Layouts.IRowLayout AddSquareButtonRight(
        this Layouts.IRowLayout rowLayout, UIFactories factories, BuilderFunc<TextButtonBuilder> builderFunc)
    {
        return rowLayout.AddRight(factories.Button(builderFunc).WrapCentered(Height, Height), Height + 2 * Margin);
    }
}
