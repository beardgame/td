using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Game.Upgrades;
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

            Game.Request(ChangePlayerState.Request(Game.Me, PlayerConnectionState.AwaitingLoadingData));

            haveModsFinishedLoading = true;
        }

        public void IntegrateUI()
        {
            var camera = new GameCamera();

            Game.IntegrateUI(camera);
        }

        private static Blueprints getHardcodedBlueprints()
        {
            var upgrades = getHardcodedUpgrades();
            var techs = getHardcodedTechnologies(upgrades);

            return new Blueprints(
                ReadonlyBlueprintCollection.Empty,
                ReadonlyBlueprintCollection.Empty,
                ReadonlyBlueprintCollection.Empty,
                ReadonlyBlueprintCollection.Empty,
                ReadonlyBlueprintCollection.Empty,
                ReadonlyBlueprintCollection.Empty,
                ReadonlyBlueprintCollection.Empty,
                ReadonlyBlueprintCollection.Empty,
                upgrades,
                techs);
        }

        private static ReadonlyBlueprintCollection<IUpgradeBlueprint> getHardcodedUpgrades()
        {
            var idManager = new IdManager();
            var collection = new BlueprintCollection<IUpgradeBlueprint>();

            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+25% damage", 50,
                new[] {new ParameterModifiable(AttributeType.Damage, Modification.AddFractionOfBase(.25))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+1 worker", 100,
                    new[] {new ParameterModifiable(AttributeType.DroneCount, Modification.AddConstant(1))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+20% resources", 75,
                new[] {new ParameterModifiable(AttributeType.ResourceIncome, Modification.AddFractionOfBase(.2))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+25% health", 80,
                new[] {new ParameterModifiable(AttributeType.Health, Modification.AddFractionOfBase(.25))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+20% range", 30,
                new[] {new ParameterModifiable(AttributeType.Range, Modification.AddFractionOfBase(.2))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+50% fire rate", 100,
                new[] {new ParameterModifiable(AttributeType.FireRate, Modification.AddFractionOfBase(.5))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "x1.5 potency", 100,
                new[] {new ParameterModifiable(AttributeType.EffectStrength, Modification.MultiplyWith(1.5))}));
            addHardcodedUpgrade(id => new UpgradeBlueprint(id, "+25% rotation speed", 60,
                new[] {new ParameterModifiable(AttributeType.TurnSpeed, Modification.AddFractionOfBase(.25))}));

            return collection.AsReadonly();

            void addHardcodedUpgrade(Func<string, UpgradeBlueprint> blueprintFactory)
            {
                collection.Add(blueprintFactory(idManager.GetNext<UpgradeBlueprint>().Value.ToString()));
            }
        }

        private static ReadonlyBlueprintCollection<ITechnologyBlueprint> getHardcodedTechnologies(
            ReadonlyBlueprintCollection<IUpgradeBlueprint> upgrades)
        {
            var idManager = new IdManager();

            var blueprintCollection = new BlueprintCollection<ITechnologyBlueprint>();

            upgrades.All
                .Select(u =>
                    new Technology(
                        idManager.GetNext<Technology>().Value.ToString(),
                        u.Name,
                        20,
                        ImmutableList.Create<ITechnologyUnlock>(new UpgradeUnlock(u))))
                .ForEach(blueprintCollection.Add);

            return blueprintCollection.AsReadonly();
        }
    }
}
