using System;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class StartManualOverdrive
{
    public static IRequest<Player, GameInstance> Request(GameObject gameObject, Faction faction) =>
        new Implementation(gameObject, faction);

    private sealed class Implementation(GameObject gameObject, Faction faction) : UnifiedRequestCommand
    {
        public override bool CheckPreconditions(Player actor) =>
            actor.Faction == faction &&
            faction.TryGetBehaviorIncludingAncestors<FactionResources>(out _) &&
            gameObject.GetComponents<IManualOverdrive>().SingleOrDefault() is { } overdrive &&
            overdrive.CanBeEnabledBy(actor.Faction);

        public override void Execute()
        {
            var overdrive = gameObject.GetComponents<IManualOverdrive>().SingleOrDefault();
            if (overdrive is null) return;

            if (!faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            {
                throw new InvalidOperationException();
            }

            resources.ConsumeResources(Constants.Game.Overdrive.OverdriveCost);
            overdrive.StartOverdrive(overdrive.EndOverdrive);
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(gameObject, faction);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<GameObject> gameObject;
        private Id<Faction> faction;

        [UsedImplicitly] public Serializer() { }

        public Serializer(GameObject gameObject, Faction faction)
        {
            this.gameObject = gameObject.FindId();
            this.faction = faction.Id;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game.State.Find(gameObject), game.State.Factions.Resolve(faction));
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref gameObject);
            stream.Serialize(ref faction);
        }
    }
}
