using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.UI.Controls
{
    // 2. We don't even need a special case for upgrades any more. Those should just be a special kind of report as
    //    well, this means that the entity status will only have a list of reports, which it can build into a
    //    scrollable column layout. The hardest thing will be that each of these reports may have to be able to open the
    //    secondary panel right of the main panel. The best way to solve this is probably using another Navigation
    //    controller.

    sealed class BuildingStatusOverlayControl : CompositeControl
    {
        private readonly BuildingStatusOverlay model;

        private readonly UpgradeOverviewControl upgradeOverview = new();
        private readonly UpgradeSelectionControl upgradeSelection = new();

        private readonly Binding<string> name = new();
        private readonly Binding<string> ownerName = new();
        private readonly Binding<Color> ownerColor = new();
        private readonly Binding<IReportSubject> reports = new();

        public BuildingStatusOverlayControl(BuildingStatusOverlay model)
        {
            IsClickThrough = true;

            this.model = model;
            this.BuildLayout()
                .ForFullScreen()
                .AddEntityStatus(b => b
                    .WithName(name)
                    .AddTextAttribute("Owned by", ownerName, ownerColor)
                    .WithReports(reports, new ReportControlFactory(model.Game, model.Pulse))
                    .WithContent(upgradeOverview)
                    .WithCloseAction(model.Close))
                .DockFixedSizeToLeft(upgradeSelection, Constants.UI.Menu.Width);

            onBuildingSet();

            model.BuildingSet += onBuildingSet;
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
            reports.SetFromSource(model.Building as IReportSubject ?? new EmptyReportSubject());
            upgradeOverview.SetUpgrades(model.BuildingUpgrades, model.CanPlayerUpgradeBuilding);
            onUpgradesUpdated();
        }

        private void onUpgradesUpdated()
        {
            upgradeOverview.UpdateAppliedUpgrades(true);
            upgradeSelection.SetUpgrades(model.AvailableUpgrades);
        }

        private void updateBuildingAttributes()
        {
            name.SetFromSource(model.Building.Blueprint.Name);
            ownerName.SetFromSource(model.Building.Faction.Name);
            ownerColor.SetFromSource(model.Building.Faction.Color);
        }

        private sealed class UpgradeOverviewControl : CompositeControl
        {
            private readonly ListControl list;
            private UpgradeOverviewItems listItems;

            private IReadOnlyCollection<BuildingStatusOverlay.BuildingUpgradeModel> upgrades
                = new List<BuildingStatusOverlay.BuildingUpgradeModel>().AsReadOnly();
            private bool canUpgrade;

            public event VoidEventHandler? ChooseUpgradeButtonClicked;

            public UpgradeOverviewControl()
            {
                list = new ListControl(new ViewportClippingLayerControl());
                listItems = new UpgradeOverviewItems(upgrades.ToImmutableArray(), canUpgrade);
                listItems.ChooseUpgradeButtonClicked += onChooseUpgradeButtonClicked;
                Add(list);
            }

            public void SetUpgrades(
                IReadOnlyCollection<BuildingStatusOverlay.BuildingUpgradeModel> upgrades, bool canUpgrade)
            {
                this.upgrades = upgrades;
                this.canUpgrade = canUpgrade;
                UpdateAppliedUpgrades(true);
            }

            public void UpdateAppliedUpgrades(bool forceReload = false)
            {
                var newUpgrades = upgrades.ToImmutableArray();

                if (listItems.ItemCount - 1 == newUpgrades.Length && !forceReload)
                {
                    return;
                }

                listItems.ChooseUpgradeButtonClicked -= onChooseUpgradeButtonClicked;
                listItems.DestroyAll();
                listItems = new UpgradeOverviewItems(newUpgrades, canUpgrade);
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
            private readonly Dictionary<int, Binding<double>> progressBindings = new();
            private readonly ImmutableArray<BuildingStatusOverlay.BuildingUpgradeModel> appliedUpgrades;
            private readonly bool canUpgrade;

            public int ItemCount => appliedUpgrades.Length + 1;

            public event VoidEventHandler? ChooseUpgradeButtonClicked;

            public UpgradeOverviewItems(
                ImmutableArray<BuildingStatusOverlay.BuildingUpgradeModel> upgrades, bool canUpgrade)
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
                        b.WithLabel("Choose upgrade").WithOnClick(() => ChooseUpgradeButtonClicked?.Invoke());
                        if (!canUpgrade)
                        {
                            b.MakeDisabled();
                        }
                        return b;
                    });
                }

                var model = appliedUpgrades[index];
                var progressBinding = model.IsFinished ? null : Binding.Create(model.Progress);
                if (progressBinding != null)
                {
                    model.ProgressUpdated += progressBinding.SetFromSource;
                    progressBindings[index] = progressBinding;
                }
                return ButtonFactories.Button(b =>
                {
                    b.WithLabel(model.Blueprint.Name);
                    if (progressBinding == null)
                    {
                        b.MakeDisabled();
                    }
                    else
                    {
                        b.WithProgressBar(progressBinding);
                    }
                    return b;
                });
            }

            public void DestroyAll()
            {
                foreach (var (i, binding) in progressBindings)
                {
                    appliedUpgrades[i].ProgressUpdated -= binding.SetFromSource;
                }
                progressBindings.Clear();
            }

            public void DestroyItemControlAt(int index, Control control)
            {
                if (progressBindings.TryGetValue(index, out var binding))
                {
                    appliedUpgrades[index].ProgressUpdated -= binding.SetFromSource;
                    progressBindings.Remove(index);
                }
            }
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
