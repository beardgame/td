using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class GameStatusUIControl : CompositeControl
{
    private const double technologyButtonSize = 128;

    private readonly GameStatusUI model;
    private readonly Binding<string> resourcesAmount = new();

    public GameStatusUIControl(GameUIController gameUIController, GameStatusUI model)
    {
        this.model = model;

        var contentBox = new BackgroundBox();
        var content = new CompositeControl();
        content.BuildFixedColumn()
            .AddHeader($"{model.FactionName}", model.FactionColor)
            .AddValueLabel(
                "Resources:", resourcesAmount, rightColor: Binding.Create(Constants.Game.GameUI.ResourcesColor));
        contentBox.BuildLayout().ForInnerContent().FillContent(content);

        this.BuildLayout()
            .ForContentBox()
            .DockFixedSizeToTop(
                ButtonFactories.StandaloneIconButton(b => b
                        .WithIcon(Constants.Content.CoreUI.Sprites.Technology)
                        .WithCustomSize(technologyButtonSize)
                        .WithOnClick(gameUIController.ShowTechnologyModal))
                    .WrapAligned(technologyButtonSize, technologyButtonSize, 1, 0.5),
                technologyButtonSize + 2 * Constants.UI.Button.Margin)
            .FillContent(contentBox);

        model.StatusChanged += updateLabels;
    }

    private void updateLabels()
    {
        if (model.FactionResources == model.FactionResourcesAfterReservation)
        {
            resourcesAmount.SetFromSource($"{model.FactionResources.NumericValue}");
        }
        else
        {
            resourcesAmount.SetFromSource(
                $"{model.FactionResources.NumericValue} -> " +
                $"{model.FactionResourcesAfterReservation.NumericValue}");
        }
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
