using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class FireEmergencyEMP
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, GameObject obj) =>
        new Implementation(obj);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameObject obj;

        public Implementation(GameObject obj)
        {
            this.obj = obj;
        }

        public override bool CheckPreconditions(Player actor)
            => obj.TryGetSingleComponent<EmergencyEMP>(out var emp) && emp.Available;

        public override ISerializableCommand<GameInstance> ToCommand() => this;

        public override void Execute()
        {
            obj.GetComponents<EmergencyEMP>().First().Fire();
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() =>
            new Serializer(obj);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<GameObject> id;

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(GameObject obj)
        {
            id = obj.FindId();
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game.State.Find(id));
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref id);
        }
    }
}
