using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed partial class UpgradeReportControl
    {
        private sealed class UpgradeDetailsControl : CompositeControl
        {
            private readonly CompositeControl buttonContainer = new();
            private readonly IUpgradeReportInstance reportInstance;

            public UpgradeDetailsControl(IUpgradeReportInstance reportInstance)
            {
                this.reportInstance = reportInstance;
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
                    layout.Add(
                        ButtonFactories.Button(b => b
                            .WithLabel(u.Name)
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
}
