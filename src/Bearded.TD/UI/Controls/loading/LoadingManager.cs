using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Game.World;
using Bearded.TD.Mods;
using Bearded.TD.Mods.Models;
using Bearded.TD.Networking;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;

namespace Bearded.TD.UI.Controls
{
    abstract class LoadingManager
    {
        public GameInstance Game { get; }
        public NetworkInterface Network { get; }
        protected IDispatcher<GameInstance> Dispatcher => Game.Meta.Dispatcher;
        protected Logger Logger => Game.Meta.Logger;
        private readonly List<ModForLoading> modsForLoading = new List<ModForLoading>();

        private bool hasModsQueuedForLoading => modsForLoading.Count > 0;
        private bool haveModsFinishedLoading;

        protected LoadingManager(GameInstance game, NetworkInterface networkInterface)
        {
            Game = game;
            Network = networkInterface;
        }

        public virtual void Update(UpdateEventArgs args)
        {
            // Network handling.
            Network.ConsumeMessages();
            Game.UpdatePlayers(args);

            // Mod loading.
            if (!hasModsQueuedForLoading)
            {
                DebugAssert.State.Satisfies(Game.Me.ConnectionState == PlayerConnectionState.LoadingMods);
                Game.ContentManager.Mods.ForEach(loadMod);
            }
            
            if (!haveModsFinishedLoading && modsForLoading.All(mod => mod.IsDone))
            {
                gatherModBlueprints();
            }
        }

        private void loadMod(ModMetadata modMetadata)
        {
            var modForLoading = modMetadata.PrepareForLoading();
            var context = new ModLoadingContext(Logger, Game.ContentManager.GraphicsLoader);
            modForLoading.StartLoading(context);
            modsForLoading.Add(modForLoading);
        }

        private void gatherModBlueprints()
        {
            Game.SetBlueprints(Blueprints.Merge(
                modsForLoading
                    .Select(modForLoading => modForLoading.GetLoadedMod())
                    .Select(mod => mod.Blueprints)
                    .Append(getHardcodedBlueprints())));

            Game.RequestDispatcher.Dispatch(
                ChangePlayerState.Request(Game.Me, PlayerConnectionState.AwaitingLoadingData));

            haveModsFinishedLoading = true;
        }

        public void IntegrateUI()
        {
            var camera = new GameCamera();

            Game.IntegrateUI(camera);
        }

        private static Blueprints getHardcodedBlueprints()
        {
            return new Blueprints(
                new ReadonlyBlueprintCollection<SpriteSet>(Enumerable.Empty<SpriteSet>()),
                new ReadonlyBlueprintCollection<FootprintGroup>(Enumerable.Empty<FootprintGroup>()),
                new ReadonlyBlueprintCollection<IBuildingBlueprint>(Enumerable.Empty<IBuildingBlueprint>()),
                new ReadonlyBlueprintCollection<IUnitBlueprint>(Enumerable.Empty<IUnitBlueprint>()),
                new ReadonlyBlueprintCollection<IWeaponBlueprint>(Enumerable.Empty<IWeaponBlueprint>()),
                new ReadonlyBlueprintCollection<IProjectileBlueprint>(Enumerable.Empty<IProjectileBlueprint>()),
                getHardcodedUpgrades());
        }
        
        private static ImmutableDictionary<Id<UpgradeBlueprint>, UpgradeBlueprint> getHardcodedUpgrades()
        {
            var idManager = new IdManager();
            var builder = ImmutableDictionary.CreateBuilder<Id<UpgradeBlueprint>, UpgradeBlueprint>();
            
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+25% damage", 50,
                new[] {new ParameterModifiable(AttributeType.Damage, Modification.AddFractionOfBase(.25))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+1 worker", 100,
                    new[] {new ParameterModifiable(AttributeType.DroneCount, Modification.AddConstant(1))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+20% resources", 75,
                new[] {new ParameterModifiable(AttributeType.ResourceIncome, Modification.AddFractionOfBase(.2))}));
            
            return builder.ToImmutable();

            void addHardcodedUpgrade(Func<Id<UpgradeBlueprint>, UpgradeBlueprint> blueprintFactory)
            {
                var id = idManager.GetNext<UpgradeBlueprint>();
                builder.Add(id, blueprintFactory(id));
            }
        }
    }
}
