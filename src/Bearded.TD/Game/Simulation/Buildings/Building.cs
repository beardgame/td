﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Statistics;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings
{
    [ComponentOwner]
    sealed class Building :
        PlacedBuildingBase<Building>,
        IIdable<Building>,
        IDamageSource,
        IListener<ReportAdded>,
        IMortal,
        IReportSubject,
        ISyncable,
        IUpgradable
    {
        private readonly List<BuildingUpgradeTask> upgradesInProgress = new();
        public ReadOnlyCollection<BuildingUpgradeTask> UpgradesInProgress { get; }
        private readonly List<IUpgradeBlueprint> appliedUpgrades = new();
        public ReadOnlyCollection<IUpgradeBlueprint> AppliedUpgrades { get; }

        public Id<Building> Id { get; }

        private readonly DamageExecutor damageExecutor;
        private readonly List<IReport> reports = new();
        private ImmutableArray<ISyncable> syncables;

        public IReadOnlyCollection<IReport> Reports { get; }
        private bool isDead;

        // TODO: ideally this is not exposed to components, but it's needed by worker tasks...
        public bool IsBuildCompleted => MutableState.IsCompleted;

        public event VoidEventHandler? Completing;

        public Building(Id<Building> id, IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            Id = id;
            AppliedUpgrades = appliedUpgrades.AsReadOnly();
            UpgradesInProgress = upgradesInProgress.AsReadOnly();
            damageExecutor = new DamageExecutor(Events);
            Reports = reports.AsReadOnly();
            reports.Add(new UpgradeReport(this));
        }

        public void AddComponent(IComponent<Building> component)
        {
            Components.Add(component);
        }

        protected override IEnumerable<IComponent<Building>> InitializeComponents()
            => Blueprint.GetComponentsForBuilding().Append(new StatisticCollector<Building>());

        public void AttributeDamage(IMortal target, DamageResult damageResult)
        {
            Events.Send(new CausedDamage(target, damageResult));
        }

        public void AttributeKill(IMortal target)
        {
            Events.Send(new CausedKill(target));
        }

        public DamageResult Damage(DamageInfo damage)
        {
            return damageExecutor.Damage(damage);
        }

        public void HandleEvent(ReportAdded @event)
        {
            // Use an Insert to ensure that the upgrades report is always last.
            reports.Insert(reports.Count - 1, @event.Report);
        }

        public void OnDeath()
        {
            isDead = true;
        }

        protected override void OnAdded()
        {
            Game.IdAs(this);
            Events.Subscribe(this);
            base.OnAdded();
        }

        public void Materialize()
        {
            MutableState.HasStartedBuilding = true;

            Game.Meta.Synchronizer.RegisterSyncable(this);
            syncables = Components.Get<ISyncable>().ToImmutableArray();
            OccupiedTiles.ForEach(tile => Game.Navigator.AddBackupSink(tile));
        }

        public void SetBuildProgress(HitPoints healthAdded)
        {
            DebugAssert.State.Satisfies(MutableState.IsMaterialized, "Cannot set build progress when building not materialized.");
            DebugAssert.State.Satisfies(!MutableState.IsCompleted, "Cannot update build progress after building is completed.");
            Events.Send(new HealDamage(healthAdded));
        }

        public void SetBuildCompleted()
        {
            DebugAssert.State.Satisfies(MutableState.IsMaterialized, "Cannot set build progress when building not materialized.");
            DebugAssert.State.Satisfies(!MutableState.IsCompleted, "Cannot complete building more than once.");
            Completing?.Invoke();
            MutableState.IsCompleted = true;
            Game.Meta.Events.Send(new BuildingConstructionFinished(this));
        }

        public bool CanBeUpgradedBy(Faction faction) => faction.SharesBehaviorWith<FactionResources>(Faction);

        public bool CanApplyUpgrade(IUpgradeBlueprint upgrade)
        {
            return !appliedUpgrades.Contains(upgrade) && upgrade.CanApplyTo(Components);
        }

        public void ApplyUpgrade(IUpgradeBlueprint upgrade)
        {
            upgrade.ApplyTo(Components);

            appliedUpgrades.Add(upgrade);
            Game.Meta.Events.Send(new BuildingUpgradeFinished(this, upgrade));
        }

        public void RegisterBuildingUpgradeTask(BuildingUpgradeTask task)
        {
            Argument.Satisfies(task.Building == this, "Can only add tasks upgrading this building.");
            Argument.Satisfies(
                upgradesInProgress.All(t => t.Upgrade != task.Upgrade),
                "Cannot queue an upgrade task for the same upgrade twice.");
            upgradesInProgress.Add(task);
        }

        public void UnregisterBuildingUpgradeTask(BuildingUpgradeTask task)
        {
            var wasRemoved = upgradesInProgress.Remove(task);

            Argument.Satisfies(wasRemoved, "Can only remove task that was added previously.");
        }

        protected override void OnDelete()
        {
            OccupiedTiles.ForEach(tile =>
            {
                Game.Navigator.RemoveSink(tile);
            });

            base.OnDelete();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if (isDead)
            {
                this.Sync(KillBuilding.Command);
            }
        }

        public override void Draw(CoreDrawers drawers)
        {
            if (!MutableState.IsMaterialized)
            {
                return;
            }

            base.Draw(drawers);
        }

        public IEnumerable<IUpgradeBlueprint> GetApplicableUpgrades() =>
            Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology)
                ? technology.GetApplicableUpgradesFor(this)
                : Enumerable.Empty<IUpgradeBlueprint>();

        public IStateToSync GetCurrentStateToSync() =>
            new CompositeStateToSync(syncables.Select(s => s.GetCurrentStateToSync()));
    }
}
