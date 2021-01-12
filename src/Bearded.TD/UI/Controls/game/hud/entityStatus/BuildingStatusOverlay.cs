using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities.Collections;
using Bearded.UI.Navigation;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class BuildingStatusOverlay
        : UpdateableNavigationNode<IPlacedBuilding>,
            IListener<BuildingUpgradeQueued>,
            IListener<UpgradeTechnologyUnlocked>
    {
        private GameInstance? game;
        private Faction? playerFaction;

        public IPlacedBuilding Building { get; private set; } = null!;

        // TODO: invoke this event when the placeholder is replaced by the building instead of closing overlay
        public event VoidEventHandler? BuildingSet;
        public event VoidEventHandler? BuildingUpdated;
        public event VoidEventHandler? UpgradesUpdated;

        public (int CurrentHealth, int MaxHealth)? BuildingHealth
        {
            get
            {
                if (!(Building is Building b))
                {
                    return null;
                }

                var health = b.GetComponents<Health<Building>>().FirstOrDefault();
                if (health == null)
                {
                    return null;
                }

                return (health.CurrentHealth, health.MaxHealth);
            }
        }

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

        protected override void Initialize(DependencyResolver dependencies, IPlacedBuilding building)
        {
            base.Initialize(dependencies, building);
            Building = building;
            Building.Deleting += Close;

            game = dependencies.Resolve<GameInstance>();
            playerFaction = game.Me.Faction;

            game.Meta.Events.Subscribe<BuildingUpgradeQueued>(this);
            game.Meta.Events.Subscribe<UpgradeTechnologyUnlocked>(this);
        }

        public override void Update(UpdateEventArgs args)
        {
            BuildingUpdated?.Invoke();
        }

        public void HandleEvent(UpgradeTechnologyUnlocked @event)
        {
            UpgradesUpdated?.Invoke();
        }

        public void HandleEvent(BuildingUpgradeQueued @event)
        {
            if (@event.Building == Building)
            {
                UpgradesUpdated?.Invoke();
            }
        }

        public void QueueUpgrade(IUpgradeBlueprint upgrade)
        {
            if (game == null || !(Building is Building building))
            {
                throw new InvalidOperationException();
            }

            game.Request(UpgradeBuilding.Request, building, upgrade);
        }

        public override void Terminate()
        {
            Building.Deleting -= Close;

            game?.Meta.Events.Unsubscribe<BuildingUpgradeQueued>(this);
            game?.Meta.Events.Unsubscribe<UpgradeTechnologyUnlocked>(this);

            base.Terminate();
        }

        public void Close()
        {
            Navigation.Exit();
        }
    }
}
