using System.Collections.Immutable;
using amulware.Graphics;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusOverlayControl : CompositeControl
    {
        private readonly BuildingStatusOverlay model;

        private readonly UpgradeOverviewControl upgradeOverview = new();

        private readonly Binding<string> name = new();
        private readonly Binding<string> ownerName = new();
        private readonly Binding<Color> ownerColor = new();
        private readonly Binding<string> health = new();

        public BuildingStatusOverlayControl(BuildingStatusOverlay model)
        {
            IsClickThrough = true;

            this.model = model;
            this.BuildLayout()
                .ForFullScreen()
                .AddEntityStatus(b => b
                    .WithName(name)
                    .AddTextAttribute("Owned by", ownerName, ownerColor)
                    .AddTextAttribute("Health", health)
                    .WithContent(upgradeOverview)
                    .WithCloseAction(model.Close));

            onBuildingSet();

            model.BuildingSet += onBuildingSet;
            model.BuildingUpdated += onBuildingUpdated;

            upgradeOverview.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
        }

        private void onChooseUpgradeButtonClicked()
        {
            throw new System.NotImplementedException();
        }

        private void onBuildingSet()
        {
            updateBuildingAttributes();
            upgradeOverview.SetBuilding(model.Building as Building);
            onBuildingUpdated();
        }

        private void onBuildingUpdated()
        {
            updateHealth();
            upgradeOverview.UpdateAppliedUpgrades();
        }

        private void updateBuildingAttributes()
        {
            name.SetFromSource(model.Building.Blueprint.Name);
            ownerName.SetFromSource(model.Building.Faction.Name);
            ownerColor.SetFromSource(model.Building.Faction.Color);
        }

        private void updateHealth()
        {
            var h = model.BuildingHealth;
            health.SetFromSource(h == null ? "N/A" : $"{h.Value.CurrentHealth} / {h.Value.MaxHealth}");
        }

        private sealed class UpgradeOverviewControl : CompositeControl
        {
            private readonly ListControl list;
            private UpgradeOverviewItems listItems;

            private Building? building;
            private ImmutableArray<IUpgradeBlueprint> appliedUpgrades = ImmutableArray<IUpgradeBlueprint>.Empty;

            public event VoidEventHandler? ChooseUpgradeButtonClicked;

            public UpgradeOverviewControl()
            {
                list = new ListControl(new ViewportClippingLayerControl());
                listItems = new UpgradeOverviewItems(appliedUpgrades);
                listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
                Add(list);
            }

            public void SetBuilding(Building? building)
            {
                this.building = building;
                UpdateAppliedUpgrades(true);
            }

            public void UpdateAppliedUpgrades(bool forceReload = false)
            {
                var oldAppliedUpgradesCount = appliedUpgrades.Length;
                appliedUpgrades =
                    building?.AppliedUpgrades.ToImmutableArray() ?? ImmutableArray<IUpgradeBlueprint>.Empty;

                if (oldAppliedUpgradesCount == appliedUpgrades.Length && !forceReload)
                {
                    return;
                }

                listItems.ChooseUpgradeButtonClicked -= onChooseUpgradeButtonClicked;
                listItems = new UpgradeOverviewItems(appliedUpgrades);
                listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
                list.ItemSource = listItems;
            }

            private void onChooseUpgradeButtonClicked()
            {
                ChooseUpgradeButtonClicked?.Invoke();
            }
        }

        private sealed class UpgradeOverviewItems : IListItemSource
        {
            private readonly ImmutableArray<IUpgradeBlueprint> appliedUpgrades;

            public int ItemCount => appliedUpgrades.Length + 1;

            public event VoidEventHandler? ChooseUpgradeButtonClicked;

            public UpgradeOverviewItems(ImmutableArray<IUpgradeBlueprint> upgrades)
            {
                appliedUpgrades = upgrades;
            }

            public double HeightOfItemAt(int index) => Constants.UI.Button.Height;

            public Control CreateItemControlFor(int index)
            {
                if (index == appliedUpgrades.Length)
                {
                    // TODO: don't show button if you can't upgrade this building
                    // TODO: it should be possible to queue upgrades for building placeholders
                    return ButtonFactories.Button(b => b
                        .WithLabel("Choose upgrade")
                        .WithOnClick(() => ChooseUpgradeButtonClicked?.Invoke()));
                }

                // TODO: show queued upgrades

                return ButtonFactories.Button(b => b.WithLabel(appliedUpgrades[index].Name).MakeDisabled());
            }

            public void DestroyItemControlAt(int index, Control control) { }
        }

        private sealed class UpgradeSelectionControl : CompositeControl
        {
            // TODO: figure out how to have standardized buttons with a cost

            public UpgradeSelectionControl()
            {

            }
        }
    }
}
