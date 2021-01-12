using System.Collections.Generic;
using System.Collections.Immutable;
using amulware.Graphics;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusOverlayControl : CompositeControl
    {
        private readonly BuildingStatusOverlay model;

        private readonly UpgradeOverviewControl upgradeOverview = new();
        private readonly UpgradeSelectionControl upgradeSelection = new();

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
                    .WithCloseAction(model.Close))
                .DockFixedSizeToLeft(upgradeSelection, Constants.UI.Menu.Width);

            onBuildingSet();

            model.BuildingSet += onBuildingSet;
            model.BuildingUpdated += onBuildingUpdated;
            model.UpgradesUpdated += onUpgradesUpdated;

            upgradeOverview.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
            upgradeSelection.UpgradeSelected += onUpgradeSelected;
        }

        private void onChooseUpgradeButtonClicked()
        {
            State.Satisfies(model.CanPlayerUpgradeBuilding);

            upgradeSelection.IsVisible = !upgradeSelection.IsVisible;
        }

        private void onUpgradeSelected(IUpgradeBlueprint upgrade)
        {
            State.Satisfies(model.CanPlayerUpgradeBuilding);

            model.QueueUpgrade(upgrade);
        }

        private void onBuildingSet()
        {
            upgradeSelection.IsVisible = false;

            updateBuildingAttributes();
            upgradeOverview.SetBuilding(model.Building as Building, model.CanPlayerUpgradeBuilding);
            onBuildingUpdated();
            onUpgradesUpdated();
        }

        private void onBuildingUpdated()
        {
            updateHealth();
            upgradeOverview.UpdateAppliedUpgrades();
        }

        private void onUpgradesUpdated()
        {
            upgradeSelection.SetUpgrades(model.AvailableUpgrades);
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
            private bool canUpgrade;

            public event VoidEventHandler? ChooseUpgradeButtonClicked;

            public UpgradeOverviewControl()
            {
                list = new ListControl(new ViewportClippingLayerControl());
                listItems = new UpgradeOverviewItems(appliedUpgrades, canUpgrade);
                listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
                Add(list);
            }

            public void SetBuilding(Building? building, bool canUpgrade)
            {
                this.building = building;
                this.canUpgrade = canUpgrade;
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
                listItems = new UpgradeOverviewItems(appliedUpgrades, canUpgrade);
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
            private readonly bool canUpgrade;

            public int ItemCount => appliedUpgrades.Length + 1;

            public event VoidEventHandler? ChooseUpgradeButtonClicked;

            public UpgradeOverviewItems(ImmutableArray<IUpgradeBlueprint> upgrades, bool canUpgrade)
            {
                appliedUpgrades = upgrades;
                this.canUpgrade = canUpgrade;
            }

            public double HeightOfItemAt(int index) => Constants.UI.Button.Height;

            public Control CreateItemControlFor(int index)
            {
                if (index == appliedUpgrades.Length)
                {
                    // TODO: it should be possible to queue upgrades for building placeholders
                    return ButtonFactories.Button(b =>
                    {
                        b
                            .WithLabel("Choose upgrade")
                            .WithOnClick(() => ChooseUpgradeButtonClicked?.Invoke());
                        if (!canUpgrade)
                        {
                            b.MakeDisabled();
                        }
                        return b;
                    });
                }

                // TODO: show queued upgrades

                return ButtonFactories.Button(b => b.WithLabel(appliedUpgrades[index].Name).MakeDisabled());
            }

            public void DestroyItemControlAt(int index, Control control) { }
        }

        private sealed class UpgradeSelectionControl : CompositeControl
        {
            private readonly CompositeControl buttonContainer = new();

            public event GenericEventHandler<IUpgradeBlueprint>? UpgradeSelected;

            public UpgradeSelectionControl()
            {
                Add(new BackgroundBox());
                this.BuildLayout()
                    .ForContentBox()
                    .FillContent(buttonContainer);
            }

            public void SetUpgrades(IEnumerable<IUpgradeBlueprint> upgrades)
            {
                buttonContainer.RemoveAllChildren();

                var layout = buttonContainer.BuildScrollableColumn();
                foreach (var u in upgrades)
                {
                    layout.Add(
                        ButtonFactories.Button(
                            b => b.WithLabel(u.Name).WithResourceCost(u.Cost).WithOnClick(() => onUpgradeClicked(u))),
                        Constants.UI.Button.Height);
                }
            }

            private void onUpgradeClicked(IUpgradeBlueprint upgrade)
            {
                UpgradeSelected?.Invoke(upgrade);
            }
        }
    }
}
