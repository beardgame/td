using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Elements;

static class SetDischargeMode
{
    public static IRequest<Player, GameInstance> Request(GameObject gameObject, CapacitorDischargeMode dischargeMode) =>
        new Implementation(gameObject, dischargeMode);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameObject gameObject;
        private readonly CapacitorDischargeMode dischargeMode;

        public Implementation(GameObject gameObject, CapacitorDischargeMode dischargeMode)
        {
            this.gameObject = gameObject;
            this.dischargeMode = dischargeMode;
        }

        public override bool CheckPreconditions(Player actor) =>
            gameObject.GetComponents<IDischargeModeSetter>().SingleOrDefault() is not null;

        public override void Execute()
        {
            if (gameObject.GetComponents<IDischargeModeSetter>().SingleOrDefault() is { } discharger)
            {
                discharger.SetDischargeMode(dischargeMode);
            }
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(gameObject, dischargeMode);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<GameObject> gameObject;
        private CapacitorDischargeMode dischargeMode;

        // ReSharper disable once UnusedMember.Local
        public Serializer() { }

        public Serializer(GameObject gameObject, CapacitorDischargeMode dischargeMode)
        {
            this.gameObject = gameObject.FindId();
            this.dischargeMode = dischargeMode;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game.State.Find(gameObject), dischargeMode);
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref gameObject);
            stream.Serialize(ref dischargeMode);
        }
    }
}
