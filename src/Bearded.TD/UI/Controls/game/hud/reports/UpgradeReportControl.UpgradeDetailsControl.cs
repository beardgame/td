using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed partial class UpgradeReportControl
{
    private sealed class UpgradeDetailsControl : CompositeControl
    {
        private readonly CompositeControl buttonContainer = new();
        private readonly IUpgradeReportInstance reportInstance;
        private readonly Binding<bool> canUpgrade;
        private readonly Binding<ResourceAmount> resources;

        public UpgradeDetailsControl(
            IUpgradeReportInstance reportInstance, Binding<bool> canUpgrade, Binding<ResourceAmount> resources)
        {
            this.reportInstance = reportInstance;
            this.canUpgrade = canUpgrade;
            this.resources = resources;
            this.BuildLayout()
                .ForContentBox()
                .FillContent(buttonContainer);
            reportInstance.AvailableUpgradesUpdated += onUpgradesUpdated;
            onUpgradesUpdated();
        }

        private void onUpgradesUpdated()
        {
            buttonContainer.RemoveAllChildren();

            var layout = buttonContainer.BuildScrollableColumn();
            foreach (var u in reportInstance.AvailableUpgrades)
            {
                var canAfford = resources.Transform(r => r >= u.Cost);
                layout.Add(
                    ButtonFactories.Button(b => b
                        .WithLabel(u.Name)
                        .WithEnabled(canUpgrade.And(canAfford))
                        .WithResourceCost(u.Cost)
                        .WithOnClick(() => reportInstance.QueueUpgrade(u))),
                    Constants.UI.Button.Height);
            }
        }

        public void Dispose()
        {
            reportInstance.AvailableUpgradesUpdated -= onUpgradesUpdated;
        }
    }
}
