using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls
{
    sealed partial class UpgradeReportControl : ReportControl
    {
        public override double Height { get; } = 200;

        private readonly IUpgradeReportInstance reportInstance;
        private readonly ControlContainer controlContainer;

        private readonly ListControl list;
        private UpgradeListItems listItems;
        private bool isDetailsOpen;
        private bool isAddedToParent;

        public UpgradeReportControl(IUpgradeReportInstance reportInstance, ControlContainer controlContainer)
        {
            this.reportInstance = reportInstance;
            this.controlContainer = controlContainer;
            reportInstance.UpgradesUpdated += updateUpgradesFromReport;

            list = new ListControl(new ViewportClippingLayerControl());
            listItems = new UpgradeListItems(
                reportInstance.Upgrades.ToImmutableArray(), reportInstance.CanPlayerUpgradeBuilding);
            listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
            list.ItemSource = listItems;
            Add(list);
        }

        protected override void OnAddingToParent()
        {
            base.OnAddingToParent();
            isAddedToParent = true;
        }

        private void updateUpgradesFromReport()
        {
            var newUpgrades = reportInstance.Upgrades.ToImmutableArray();

            if (listItems.AppliedUpgrades.Length == newUpgrades.Length)
            {
                if (isAddedToParent)
                {
                    list.Reload();
                }

                return;
            }

            listItems.ChooseUpgradeButtonClicked -= onChooseUpgradeButtonClicked;
            listItems.DestroyAll();
            listItems = new UpgradeListItems(newUpgrades, reportInstance.CanPlayerUpgradeBuilding);
            listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
            list.ItemSource = listItems;
        }

        public override void Update()
        {
            listItems.UpdateProgress();
        }

        public override void Dispose()
        {
            reportInstance.Dispose();
            if (isDetailsOpen)
            {
                controlContainer.ClearControl();
            }
        }

        private void onChooseUpgradeButtonClicked()
        {
            if (isDetailsOpen)
            {
                controlContainer.ClearControl();
                return;
            }

            var details = new UpgradeDetailsControl(reportInstance);
            controlContainer.SetControl(details, () =>
            {
                details.Dispose();
                isDetailsOpen = false;
            });
            isDetailsOpen = true;
        }
    }
}
