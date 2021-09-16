using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed partial class BuildingUpgradeManager<T>
    {
        private sealed class UpgradeReport : IUpgradeReport
        {
            public ReportType Type => ReportType.Upgrades;

            private readonly BuildingUpgradeManager<T> source;

            public UpgradeReport(BuildingUpgradeManager<T> source)
            {
                this.source = source;
            }

            public IUpgradeReportInstance CreateInstance(GameInstance game)
            {
                // TODO: the cast below should really not exist
                return new UpgradeReportInstance((source.Owner as Building)!, source, game);
            }

            private sealed class UpgradeReportInstance : IUpgradeReportInstance, IListener<UpgradeTechnologyUnlocked>
            {
                private readonly Building subject;
                private readonly IBuildingUpgradeManager upgradeManager;
                private readonly GameInstance game;
                private readonly GlobalGameEvents events;
                private readonly Faction playerFaction;

                private readonly List<BuildingUpgradeModel> buildingUpgrades = new();
                private readonly List<IUpgradeBlueprint> buildingAvailableUpgrades = new();

                public IReadOnlyCollection<IUpgradeReportInstance.IUpgradeModel> Upgrades { get; }
                public IReadOnlyCollection<IUpgradeBlueprint> AvailableUpgrades { get; }

                public bool CanPlayerUpgradeBuilding => upgradeManager.CanBeUpgradedBy(playerFaction);

                public event VoidEventHandler? UpgradesUpdated;
                public event VoidEventHandler? AvailableUpgradesUpdated;

                // TODO: subject should be a IComponentGameObject, not a Building
                public UpgradeReportInstance(Building subject, IBuildingUpgradeManager upgradeManager, GameInstance game)
                {
                    this.subject = subject;
                    this.upgradeManager = upgradeManager;
                    this.game = game;
                    events = game.Meta.Events;
                    playerFaction = game.Me.Faction;

                    upgradeManager.UpgradeQueued += onUpgradeQueued;
                    upgradeManager.UpgradeCompleted += onUpgradeCompleted;
                    events.Subscribe(this);

                    var appliedUpgrades = this.upgradeManager.AppliedUpgrades;
                    var upgradesInProgress = this.upgradeManager.UpgradesInProgress;

                    var finishedUpgrades = appliedUpgrades.WhereNot(u => upgradesInProgress.Any(t => t.Upgrade == u));

                    foreach (var u in finishedUpgrades)
                    {
                        buildingUpgrades.Add(new BuildingUpgradeModel(u, null));
                    }

                    foreach (var task in upgradesInProgress)
                    {
                        buildingUpgrades.Add(new BuildingUpgradeModel(task.Upgrade, task));
                    }

                    Upgrades = buildingUpgrades.AsReadOnly();
                    AvailableUpgrades = buildingAvailableUpgrades.AsReadOnly();
                    updateAvailableUpgrades();
                }

                public void Dispose()
                {
                    upgradeManager.UpgradeQueued -= onUpgradeQueued;
                    upgradeManager.UpgradeCompleted -= onUpgradeCompleted;
                    events.Unsubscribe(this);
                }

                private void onUpgradeCompleted(IUpgradeBlueprint upgrade)
                {
                    var i = buildingUpgrades.FindIndex(model => model.Blueprint == upgrade);
                    if (i == -1)
                    {
                        State.IsInvalid();
                    }

                    buildingUpgrades[i].MarkFinished();
                    UpgradesUpdated?.Invoke();
                }

                private void onUpgradeQueued(IIncompleteUpgrade incompleteUpgrade)
                {
                    buildingUpgrades.Add(new BuildingUpgradeModel(incompleteUpgrade.Upgrade, incompleteUpgrade));
                    buildingAvailableUpgrades.Remove(incompleteUpgrade.Upgrade);
                    UpgradesUpdated?.Invoke();
                    AvailableUpgradesUpdated?.Invoke();
                }

                public void HandleEvent(UpgradeTechnologyUnlocked @event)
                {
                    if (!playerFaction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var playerTechnology)
                        || playerTechnology != @event.FactionTechnology)
                    {
                        return;
                    }

                    updateAvailableUpgrades();
                    AvailableUpgradesUpdated?.Invoke();
                }

                private void updateAvailableUpgrades()
                {
                    buildingAvailableUpgrades.Clear();
                    var upgradesInProgress =
                        upgradeManager.UpgradesInProgress.Select(t => t.Upgrade).ToImmutableHashSet();
                    buildingAvailableUpgrades
                        .AddRange(upgradeManager.ApplicableUpgrades.WhereNot(upgradesInProgress.Contains));
                }

                public void QueueUpgrade(IUpgradeBlueprint upgrade)
                {
                    game.Request(UpgradeBuilding.Request, subject, upgrade);
                }
            }

            private sealed class BuildingUpgradeModel : IUpgradeReportInstance.IUpgradeModel
            {
                private IIncompleteUpgrade? incompleteUpgrade;

                public IUpgradeBlueprint Blueprint { get; }

                public double Progress => incompleteUpgrade?.PercentageComplete ?? 1;

                public bool IsFinished => Progress >= 1;

                public BuildingUpgradeModel(IUpgradeBlueprint blueprint, IIncompleteUpgrade? incompleteUpgrade)
                {
                    Blueprint = blueprint;
                    this.incompleteUpgrade = incompleteUpgrade;
                }

                public void MarkFinished()
                {
                    incompleteUpgrade = null;
                }
            }
        }
    }
}
