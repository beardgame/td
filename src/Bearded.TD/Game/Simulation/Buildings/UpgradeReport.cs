using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class UpgradeReport : IUpgradeReport
    {
        private readonly Building building;

        public UpgradeReport(Building building)
        {
            this.building = building;
        }

        public IUpgradeReportInstance CreateInstance(GameInstance game)
        {
            return new UpgradeReportInstance(building, game);
        }

        private sealed class UpgradeReportInstance
            : IUpgradeReportInstance,
                IListener<BuildingUpgradeFinished>,
                IListener<BuildingUpgradeQueued>,
                IListener<UpgradeTechnologyUnlocked>
        {
            private readonly Building building;
            private readonly GameInstance game;
            private readonly GlobalGameEvents events;
            private readonly Faction playerFaction;

            private readonly List<BuildingUpgradeModel> buildingUpgrades = new();
            private readonly List<IUpgradeBlueprint> buildingAvailableUpgrades = new();

            public IReadOnlyCollection<IUpgradeReportInstance.IUpgradeModel> Upgrades { get; }
            public IReadOnlyCollection<IUpgradeBlueprint> AvailableUpgrades { get; }

            public bool CanPlayerUpgradeBuilding => building.CanBeUpgradedBy(playerFaction);

            public event VoidEventHandler? UpgradesUpdated;
            public event VoidEventHandler? AvailableUpgradesUpdated;

            public UpgradeReportInstance(Building building, GameInstance game)
            {
                this.building = building;
                this.game = game;
                events = game.Meta.Events;
                playerFaction = game.Me.Faction;

                events.Subscribe<BuildingUpgradeFinished>(this);
                events.Subscribe<BuildingUpgradeQueued>(this);
                events.Subscribe<UpgradeTechnologyUnlocked>(this);

                var appliedUpgrades = building.AppliedUpgrades;
                var upgradesInProgress = building.UpgradesInProgress;

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
                events.Unsubscribe<BuildingUpgradeFinished>(this);
                events.Unsubscribe<BuildingUpgradeQueued>(this);
                events.Unsubscribe<UpgradeTechnologyUnlocked>(this);
            }

            public void HandleEvent(BuildingUpgradeFinished @event)
            {
                if (@event.Building != building)
                {
                    return;
                }

                var i = buildingUpgrades.FindIndex(model => model.Blueprint == @event.Upgrade);
                if (i == -1)
                {
#if DEBUG
                    throw new InvalidOperationException();
#else
                    return;
#endif
                }

                buildingUpgrades[i].MarkFinished();
                UpgradesUpdated?.Invoke();
            }

            public void HandleEvent(BuildingUpgradeQueued @event)
            {
                if (@event.Building != building)
                {
                    return;
                }

                buildingUpgrades.Add(new BuildingUpgradeModel(@event.Upgrade, @event.Task));
                buildingAvailableUpgrades.Remove(@event.Upgrade);
                UpgradesUpdated?.Invoke();
                AvailableUpgradesUpdated?.Invoke();
            }

            public void HandleEvent(UpgradeTechnologyUnlocked @event)
            {
                updateAvailableUpgrades();
                AvailableUpgradesUpdated?.Invoke();
            }

            private void updateAvailableUpgrades()
            {
                buildingAvailableUpgrades.Clear();
                var upgradesInProgress = building.UpgradesInProgress.Select(t => t.Upgrade).ToImmutableHashSet();
                buildingAvailableUpgrades
                    .AddRange(building.GetApplicableUpgrades().WhereNot(upgradesInProgress.Contains));
            }

            public void QueueUpgrade(IUpgradeBlueprint upgrade)
            {
                game.Request(UpgradeBuilding.Request, building, upgrade);
            }
        }

        private sealed class BuildingUpgradeModel : IUpgradeReportInstance.IUpgradeModel
        {
            private BuildingUpgradeTask? task;

            public IUpgradeBlueprint Blueprint { get; }

            public double Progress => task?.ProgressPercentage ?? 1;

            public bool IsFinished => Progress >= 1;

            public BuildingUpgradeModel(IUpgradeBlueprint blueprint, BuildingUpgradeTask? task)
            {
                Blueprint = blueprint;
                this.task = task;
            }

            public void MarkFinished()
            {
                task = null;
            }
        }
    }
}
