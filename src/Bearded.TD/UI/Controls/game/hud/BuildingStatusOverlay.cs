using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.UI.Controls
{
    [UsedImplicitly]
    sealed class BuildingStatusOverlay
        : UpdateableNavigationNode<IPlacedBuilding>,
            IListener<BuildingUpgradeFinished>,
            IListener<BuildingUpgradeQueued>,
            IListener<UpgradeTechnologyUnlocked>
    {
        private GameInstance? game;
        private Faction? playerFaction;
        private Pulse pulse = null!;

        private readonly List<BuildingUpgradeModel> buildingUpgrades = new();
        private readonly List<(BuildingUpgradeTask Task, BuildingUpgradeModel Model)> monitoredBuildingUpgrades = new();

        public IPlacedBuilding Building { get; private set; } = null!;
        public IReadOnlyCollection<BuildingUpgradeModel> BuildingUpgrades { get; }

        public GameInstance Game => game!;
        public IPulse Pulse => pulse;

        // TODO: invoke this event when the placeholder is replaced by the building instead of closing overlay
        public event VoidEventHandler? BuildingSet;
        public event VoidEventHandler? UpgradesUpdated;

        public bool CanPlayerUpgradeBuilding =>
            playerFaction != null && ((Building as Building)?.CanBeUpgradedBy(playerFaction) ?? false);

        public IEnumerable<IUpgradeBlueprint> AvailableUpgrades
        {
            get
            {
                if (!(Building is Building building))
                {
                    return Enumerable.Empty<IUpgradeBlueprint>();
                }

                var upgradesInProgress = building.UpgradesInProgress.Select(t => t.Upgrade).ToImmutableHashSet();
                return building.GetApplicableUpgrades().WhereNot(upgradesInProgress.Contains);
            }
        }

        public BuildingStatusOverlay()
        {
            BuildingUpgrades = buildingUpgrades.AsReadOnly();
        }

        protected override void Initialize(DependencyResolver dependencies, IPlacedBuilding building)
        {
            base.Initialize(dependencies, building);
            Building = building;
            Building.Deleting += Close;

            game = dependencies.Resolve<GameInstance>();
            playerFaction = game.Me.Faction;
            pulse = new Pulse(game.State, 0.5.S());

            game.Meta.Events.Subscribe<BuildingUpgradeFinished>(this);
            game.Meta.Events.Subscribe<BuildingUpgradeQueued>(this);
            game.Meta.Events.Subscribe<UpgradeTechnologyUnlocked>(this);

            if (Building is not Building builtBuilding)
            {
                return;
            }

            var appliedUpgrades = builtBuilding.AppliedUpgrades;
            var upgradesInProgress = builtBuilding.UpgradesInProgress;

            var finishedUpgrades = appliedUpgrades.WhereNot(u => upgradesInProgress.Any(t => t.Upgrade == u));

            foreach (var u in finishedUpgrades)
            {
                buildingUpgrades.Add(new BuildingUpgradeModel(u, 1));
            }
            foreach (var task in upgradesInProgress)
            {
                addAndMonitorUpgrade(task);
            }
        }

        public override void Update(UpdateEventArgs args)
        {
            foreach (var (task, model) in monitoredBuildingUpgrades)
            {
                model.Progress = task.ProgressPercentage;
            }
            pulse.Update();
        }

        public void HandleEvent(UpgradeTechnologyUnlocked @event)
        {
            UpgradesUpdated?.Invoke();
        }

        public void HandleEvent(BuildingUpgradeFinished @event)
        {
            if (@event.Building != Building)
            {
                return;
            }

            var i = monitoredBuildingUpgrades.FindIndex(tuple => tuple.Task.Upgrade == @event.Upgrade);
            if (i == -1)
            {
#if DEBUG
                throw new InvalidOperationException();
#else
                    return;
#endif
            }

            monitoredBuildingUpgrades[i].Model.Progress = 1;
            monitoredBuildingUpgrades.RemoveAt(i);
            UpgradesUpdated?.Invoke();
        }

        public void HandleEvent(BuildingUpgradeQueued @event)
        {
            if (@event.Building != Building)
            {
                return;
            }

            addAndMonitorUpgrade(@event.Task);
            UpgradesUpdated?.Invoke();
        }

        public void QueueUpgrade(IUpgradeBlueprint upgrade)
        {
            if (game == null || !(Building is Building building))
            {
                throw new InvalidOperationException();
            }

            game.Request(UpgradeBuilding.Request, building, upgrade);
        }

        private void addAndMonitorUpgrade(BuildingUpgradeTask task)
        {
            var model = new BuildingUpgradeModel(task.Upgrade, task.ProgressPercentage);
            buildingUpgrades.Add(model);
            monitoredBuildingUpgrades.Add((task, model));
        }

        public override void Terminate()
        {
            Building.Deleting -= Close;

            game?.Meta.Events.Unsubscribe<BuildingUpgradeFinished>(this);
            game?.Meta.Events.Unsubscribe<BuildingUpgradeQueued>(this);
            game?.Meta.Events.Unsubscribe<UpgradeTechnologyUnlocked>(this);

            base.Terminate();
        }

        public void Close()
        {
            Navigation.Exit();
        }

        public sealed class BuildingUpgradeModel
        {
            public IUpgradeBlueprint Blueprint { get; }

            private double progress;

            public double Progress
            {
                get => progress;
                set
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (progress == value)
                    {
                        return;
                    }

                    progress = value;
                    ProgressUpdated?.Invoke(value);
                }
            }

            public bool IsFinished => progress >= 1;

            public event GenericEventHandler<double>? ProgressUpdated;

            public BuildingUpgradeModel(IUpgradeBlueprint blueprint, double progress)
            {
                Blueprint = blueprint;
                this.progress = progress;
            }
        }
    }
}
