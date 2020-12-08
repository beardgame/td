using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.GameState.Components;
using Bearded.TD.Game.GameState.Components.Events;
using Bearded.TD.Game.GameState.Damage;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.GameState.Upgrades;
using Bearded.TD.Game.GameState.World;
using Bearded.TD.Game.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Buildings
{
    [ComponentOwner]
    class Building : PlacedBuildingBase<Building>, IIdable<Building>, IMortal, IDamageOwner
    {
        private static readonly Dictionary<SelectionState, Color> drawColors = new Dictionary<SelectionState, Color>
        {
            {SelectionState.Default, Color.Blue},
            {SelectionState.Focused, Color.DarkBlue},
            {SelectionState.Selected, Color.RoyalBlue}
        };

        private readonly List<BuildingUpgradeTask> upgradesInProgress = new List<BuildingUpgradeTask>();
        public ReadOnlyCollection<BuildingUpgradeTask> UpgradesInProgress { get; }
        private readonly List<IUpgradeBlueprint> appliedUpgrades = new List<IUpgradeBlueprint>();
        public ReadOnlyCollection<IUpgradeBlueprint> AppliedUpgrades { get; }

        public Id<Building> Id { get; }

        public bool IsCompleted { get; private set; }
        private double buildProgress;
        private bool isDead;

        public event VoidEventHandler? Completing;
        public event GenericEventHandler<int>? Healed;

        public Building(Id<Building> id, IBuildingBlueprint blueprint, Faction faction, PositionedFootprint footprint)
            : base(blueprint, faction, footprint)
        {
            Id = id;
            AppliedUpgrades = appliedUpgrades.AsReadOnly();
            UpgradesInProgress = upgradesInProgress.AsReadOnly();
        }

        protected override IEnumerable<IComponent<Building>> InitialiseComponents()
            => Blueprint.GetComponentsForBuilding();

        public void Damage(DamageInfo damage)
        {
            Events.Send(new TakeDamage(damage));
        }

        public void OnDeath()
        {
            isDead = true;
        }

        protected override void OnAdded()
        {
            Game.IdAs(this);

            OccupiedTiles.ForEach(tile => Game.Navigator.AddBackupSink(tile));

            base.OnAdded();
        }

        public void SetBuildProgress(double newBuildProgress, int healthAdded)
        {
            DebugAssert.State.Satisfies(!IsCompleted, "Cannot update build progress after building is completed.");
            buildProgress = newBuildProgress;
            Healed?.Invoke(healthAdded);
        }

        public void SetBuildCompleted()
        {
            DebugAssert.State.Satisfies(!IsCompleted, "Cannot complete building more than once.");
            Completing?.Invoke();
            IsCompleted = true;
            Game.Meta.Events.Send(new BuildingConstructionFinished(this));
        }

        public bool CanBeUpgradedBy(Faction faction) => faction.SharesResourcesWith(Faction);

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
            DebugAssert.Argument.Satisfies(task.Building == this, "Can only add tasks upgrading this building.");
            DebugAssert.Argument.Satisfies(
                upgradesInProgress.All(t => t.Upgrade != task.Upgrade),
                "Cannot queue an upgrade task for the same upgrade twice.");
            upgradesInProgress.Add(task);
        }

        public void UnregisterBuildingUpgradeTask(BuildingUpgradeTask task)
        {
            var wasRemoved = upgradesInProgress.Remove(task);

            DebugAssert.Argument.Satisfies(wasRemoved, "Can only remove task that was added previously.");
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

        public override void Draw(GeometryManager geometries)
        {
            var alpha = IsCompleted ? 1 : (float)(buildProgress * 0.9);
            //DrawTiles(geometries, drawColors[SelectionState] * alpha);
            //DrawBuildingName(geometries, Color.Black);
            base.Draw(geometries);
        }

        public IEnumerable<IUpgradeBlueprint> GetApplicableUpgrades() =>
            Faction.Technology.GetApplicableUpgradesFor(this);
    }
}
