using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed partial class UpgradeReportControl : ReportControl
{
    public override double Height => 200;

    private readonly IUpgradeReportInstance reportInstance;
    private readonly ControlContainer controlContainer;

    private readonly ListControl list;
    private UpgradeListItems listItems;
    private bool isDetailsOpen;
    private bool isAddedToParent;

    private readonly Binding<string> slots = new();
    private readonly Binding<bool> canUpgrade = new();
    private readonly Binding<ResourceAmount> resources = new();

    public UpgradeReportControl(IUpgradeReportInstance reportInstance, ControlContainer controlContainer)
    {
        this.reportInstance = reportInstance;
        this.controlContainer = controlContainer;
        reportInstance.UpgradesUpdated += updateUpgradesFromReport;
        reportInstance.AvailableUpgradesUpdated += updateUpgradesFromReport;

        list = new ListControl(new ViewportClippingLayerControl());
        listItems = new UpgradeListItems(
            reportInstance.Upgrades.ToImmutableArray(), canUpgrade, slots);
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
        listItems = new UpgradeListItems(newUpgrades, canUpgrade, slots);
        listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
        list.ItemSource = listItems;

        Update();
    }

    public override void Update()
    {
        canUpgrade.SetFromSource(reportInstance.CanPlayerUpgradeBuilding);
        resources.SetFromSource(reportInstance.PlayerResources);
        slots.SetFromSource($"{reportInstance.OccupiedUpgradeSlots} / {reportInstance.UnlockedUpgradeSlots}");
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

        var details = new UpgradeDetailsControl(reportInstance, canUpgrade, resources);
        controlContainer.SetControl(details, () =>
        {
            details.Dispose();
            isDetailsOpen = false;
        });
        isDetailsOpen = true;
    }
}
