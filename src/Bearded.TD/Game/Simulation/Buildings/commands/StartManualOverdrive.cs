using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class StartManualOverdrive
{
    public static IRequest<Player, GameInstance> Request(GameObject gameObject) =>
        new Implementation(gameObject);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameObject gameObject;

        public Implementation(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public override bool CheckPreconditions(Player actor) =>
            gameObject.GetComponents<IManualOverdrive>().SingleOrDefault() is { } overdrive &&
            overdrive.CanBeEnabledBy(actor.Faction);

        public override void Execute()
        {
            var overdrive = gameObject.GetComponents<IManualOverdrive>().SingleOrDefault();
            overdrive?.StartOverdrive(overdrive.EndOverdrive);
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(gameObject);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<GameObject> gameObject;

        [UsedImplicitly] public Serializer() { }

        public Serializer(GameObject gameObject)
        {
            this.gameObject = gameObject.FindId();
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game.State.Find(gameObject));
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref gameObject);
        }
    }
}
