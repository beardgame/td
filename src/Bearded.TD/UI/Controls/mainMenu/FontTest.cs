using Bearded.Graphics;
using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.UI.Controls;

sealed class FontTest : NavigationNode<Void>
{
    protected override void Initialize(DependencyResolver dependencies, Void parameters)
    {
    }

    public void Close()
    {
        Navigation?.Close(this);
    }
}

sealed class FontTextControl : CompositeControl
{
    public FontTextControl(FontTest model, UIContext uiContext)
    {
        Add(new BackgroundBox(Color.DimGray));
        var closeButton = uiContext.Factories.Button("X").Anchor(b => b.Top(8, 16).Right(8, 16));
        closeButton.Clicked += _ => model.Close();
        Add(closeButton);

        var column = this.BuildFixedColumn();
        column
            .Add(label(4), 4)
            .Add(label(6), 6)
            .Add(label(8), 8)
            .Add(label(10), 10)
            .Add(label(12), 12)
            .Add(label(14), 14)
            .Add(label(18), 18)
            .Add(label(24), 24)
            .Add(label(32), 32)
            .Add(label(48), 48)
            .Add(label(96), 96);

        this.Anchor(b => b.Left(20, 1200).Bottom(50, column.Height + 20));
    }

    private Control label(float fontSize)
    {
        return new Label
        {
            Text = $"Fontsize: {fontSize} AW QM qlt 123",
            FontSize = fontSize,
            Color = Color.White,
            TextAnchor = new Vector2d(0, 0.5f),
        };
    }
}
