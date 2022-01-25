using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.UI.Controls;

sealed class GameStatusUIControl : CompositeControl
{
    private readonly GameStatusUI model;
    private readonly Binding<string> resourcesAmount = new();
    private readonly Binding<string> waveNumber = new();
    private readonly Binding<string> timeUntilSpawn = new();
    private readonly Binding<string> waveResources = new();
    private readonly Binding<bool> canSkipWaveTimer = new();

    public GameStatusUIControl(GameUIController gameUIController, GameStatusUI model)
    {
        this.model = model;

        Add(new BackgroundBox());

        var content = new CompositeControl().Anchor(a => a.MarginAllSides(4));
        content.BuildFixedColumn()
            .AddHeader($"{model.FactionName}", model.FactionColor)
            .AddValueLabel(
                "Resources:", resourcesAmount, rightColor: Binding.Create(Constants.Game.GameUI.ResourcesColor))
            .AddButton(b => b.WithLabel("Research").WithOnClick(gameUIController.ShowTechnologyModal))
            .AddValueLabel("Wave:", waveNumber)
            .AddValueLabel("Next spawn:", timeUntilSpawn)
            .AddButton(
                b => b
                    .WithLabel("Summon Wave")
                    .WithOnClick(model.SkipWaveTimer)
                    .WithEnabled(canSkipWaveTimer))
            .AddValueLabel(
                "Resources this wave:",
                waveResources, rightColor: Binding.Create(Constants.Game.GameUI.ResourcesColor));
        this.BuildLayout().ForContentBox().FillContent(content);

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

        waveNumber.SetFromSource(model.WaveName ?? "-");
        timeUntilSpawn.SetFromSource(
            model.TimeUntilWaveSpawn == null ? "-" : model.TimeUntilWaveSpawn.Value.ToDisplayString());
        waveResources.SetFromSource(
            model.WaveResources == null ? "-" : $"{model.WaveResources.Value.NumericValue}");
        canSkipWaveTimer.SetFromSource(model.TimeUntilWaveSpawn != null);
    }

    protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
}
