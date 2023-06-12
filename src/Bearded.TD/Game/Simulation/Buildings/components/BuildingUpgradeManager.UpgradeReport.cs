using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class BuildingUpgradeManager
{
    private sealed class UpgradeReport : IUpgradeReport
    {
        public ReportType Type => ReportType.Upgrades;

        private readonly BuildingUpgradeManager source;

        public UpgradeReport(BuildingUpgradeManager source)
        {
            this.source = source;
        }

        public IUpgradeReportInstance CreateInstance(GameInstance game)
        {
            return new UpgradeReportInstance(source.Owner, source, source.upgradeSlots, game);
        }

        private sealed class UpgradeReportInstance : IUpgradeReportInstance, IListener<UpgradeTechnologyUnlocked>
        {
            private readonly GameObject subject;
            private readonly IBuildingUpgradeManager upgradeManager;
            private readonly IUpgradeSlots? upgradeSlots;
            private readonly GameInstance game;
            private readonly GlobalGameEvents events;
            private readonly Faction playerFaction;

            private readonly List<BuildingUpgradeModel> buildingUpgrades = new();
            private readonly List<IPermanentUpgrade> buildingAvailableUpgrades = new();

            public IReadOnlyCollection<IUpgradeReportInstance.IUpgradeModel> Upgrades { get; }
            public IReadOnlyCollection<IPermanentUpgrade> AvailableUpgrades { get; }
            public int OccupiedUpgradeSlots =>
                upgradeSlots == null ? 0 : upgradeSlots.FilledSlotsCount + upgradeSlots.ReservedSlotsCount;
            public int UnlockedUpgradeSlots => upgradeSlots?.TotalSlotsCount ?? 0;

            public bool CanPlayerUpgradeBuilding =>
                upgradeSlots is { HasAvailableSlot: true } && upgradeManager.CanBeUpgradedBy(playerFaction);

            public event VoidEventHandler? UpgradesUpdated;
            public event VoidEventHandler? AvailableUpgradesUpdated;

            public UpgradeReportInstance(
                GameObject subject,
                IBuildingUpgradeManager upgradeManager,
                IUpgradeSlots? upgradeSlots,
                GameInstance game)
            {
                this.subject = subject;
                this.upgradeManager = upgradeManager;
                this.upgradeSlots = upgradeSlots;
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

            private void onUpgradeCompleted(IPermanentUpgrade upgrade)
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

            public void QueueUpgrade(IPermanentUpgrade upgrade)
            {
                game.Request(UpgradeBuilding.Request, subject, upgrade);
            }
        }

        private sealed class BuildingUpgradeModel : IUpgradeReportInstance.IUpgradeModel
        {
            private IIncompleteUpgrade? incompleteUpgrade;

            public IPermanentUpgrade Blueprint { get; }

            public double Progress => incompleteUpgrade?.PercentageComplete ?? 1;

            public bool IsFinished => Progress >= 1;

            public BuildingUpgradeModel(IPermanentUpgrade blueprint, IIncompleteUpgrade? incompleteUpgrade)
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
