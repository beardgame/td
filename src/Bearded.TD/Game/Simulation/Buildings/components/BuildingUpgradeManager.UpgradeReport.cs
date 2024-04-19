using System.Collections.Generic;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed partial class BuildingUpgradeManager
{
    private sealed class UpgradeReport(BuildingUpgradeManager source) : IUpgradeReport
    {
        public ReportType Type => ReportType.Upgrades;

        public IUpgradeReportInstance CreateInstance(GameInstance game)
        {
            return new UpgradeReportInstance(source.Owner, source, game);
        }

        private sealed class UpgradeReportInstance : IUpgradeReportInstance, IListener<UpgradeTechnologyUnlocked>
        {
            private readonly GameObject subject;
            private readonly IBuildingUpgradeManager upgradeManager;
            private readonly IUpgradeSlots? upgradeSlots;
            private readonly GameInstance game;
            private readonly GlobalGameEvents events;
            private readonly Faction playerFaction;
            private readonly FactionResources? factionResources;

            private readonly List<IPermanentUpgrade> buildingUpgrades = [];
            private readonly List<IPermanentUpgrade> buildingAvailableUpgrades = [];

            public IReadOnlyCollection<IPermanentUpgrade> Upgrades { get; }
            public IReadOnlyCollection<IPermanentUpgrade> AvailableUpgrades { get; }
            public int OccupiedUpgradeSlots => upgradeSlots?.FilledSlotsCount ?? 0;
            public int UnlockedUpgradeSlots => upgradeSlots?.TotalSlotsCount ?? 0;

            public bool CanPlayerUpgradeBuilding =>
                upgradeSlots is { HasAvailableSlot: true } && upgradeManager.CanBeUpgradedBy(playerFaction);

            public ResourceAmount PlayerResources => (factionResources != null ? factionResources.CurrentResources : (ResourceAmount?)null) ?? ResourceAmount.Zero;

            public event VoidEventHandler? UpgradesUpdated;
            public event VoidEventHandler? AvailableUpgradesUpdated;

            public UpgradeReportInstance(
                GameObject subject,
                IBuildingUpgradeManager upgradeManager,
                GameInstance game)
            {
                this.subject = subject;
                this.upgradeManager = upgradeManager;
                subject.TryGetSingleComponent(out upgradeSlots);
                this.game = game;
                events = game.Meta.Events;
                playerFaction = game.Me.Faction;
                playerFaction.TryGetBehaviorIncludingAncestors(out factionResources);

                upgradeManager.UpgradeCompleted += onUpgradeCompleted;
                events.Subscribe(this);

                buildingUpgrades.AddRange(upgradeManager.AppliedUpgrades);
                Upgrades = buildingUpgrades.AsReadOnly();
                AvailableUpgrades = buildingAvailableUpgrades.AsReadOnly();
                updateAvailableUpgrades();
            }

            public void Dispose()
            {
                upgradeManager.UpgradeCompleted -= onUpgradeCompleted;
                events.Unsubscribe(this);
            }

            private void onUpgradeCompleted(IPermanentUpgrade upgrade)
            {
                buildingUpgrades.Add(upgrade);
                updateAvailableUpgrades();

                UpgradesUpdated?.Invoke();
            }

            public void HandleEvent(UpgradeTechnologyUnlocked @event)
            {
                if (!playerFaction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var playerTechnology)
                    || playerTechnology != @event.FactionTechnology)
                {
                    return;
                }

                updateAvailableUpgrades();
            }

            private void updateAvailableUpgrades()
            {
                buildingAvailableUpgrades.Clear();
                buildingAvailableUpgrades.AddRange(upgradeManager.ApplicableUpgrades);
                AvailableUpgradesUpdated?.Invoke();
            }

            public void ApplyUpgrade(IPermanentUpgrade upgrade)
            {
                game.Request(UpgradeBuilding.Request, subject, upgrade);
            }
        }
    }
}
