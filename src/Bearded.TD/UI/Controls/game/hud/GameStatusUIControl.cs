using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class GameStatusUIControl : CompositeControl
    {
        private readonly GameStatusUI model;
        private readonly Binding<string> resourcesAmount = new();
        private readonly Binding<string> techPointsAmount = new();
        private readonly Binding<string> waveNumber = new();
        private readonly Binding<string> timeUntilSpawn = new();
        private readonly Binding<string> waveResources = new();

        public event VoidEventHandler? TechnologyButtonClicked;

        public GameStatusUIControl(GameStatusUI model)
        {
            this.model = model;

            Add(new BackgroundBox());

            var content = new CompositeControl().Anchor(a => a.MarginAllSides(4));
            content.BuildFixedColumn()
                .AddHeader($"{model.FactionName}", model.FactionColor)
                .AddValueLabel("Resources:", resourcesAmount, rightColor: Constants.Game.GameUI.ResourcesColor)
                .AddValueLabel("Tech points:", techPointsAmount, rightColor: Constants.Game.GameUI.TechPointsColor)
                .AddButton(b => b.WithLabel("Research").WithOnClick(() => TechnologyButtonClicked?.Invoke()))
                .AddValueLabel("Wave:", waveNumber)
                .AddValueLabel("Next spawn:", timeUntilSpawn)
                .AddValueLabel("Resources this wave:", waveResources, rightColor: Constants.Game.GameUI.ResourcesColor);
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

            techPointsAmount.SetFromSource($"{model.FactionTechPoints}");
            waveNumber.SetFromSource(model.WaveName ?? "-");
            timeUntilSpawn.SetFromSource(
                model.TimeUntilWaveSpawn == null ? "-" : model.TimeUntilWaveSpawn.Value.ToDisplayString());
            waveResources.SetFromSource(
                model.WaveResources == null ? "-" : $"{model.WaveResources.Value.NumericValue}");
        }

        protected override void RenderStronglyTyped(IRendererRouter r) => r.Render(this);
    }
}
