using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class DeleteBuilding
{
    public static IRequest<Player, GameInstance> Request(GameObject building)
        => new Implementation(building);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameObject building;

        public Implementation(GameObject building)
        {
            this.building = building;
        }

        public override bool CheckPreconditions(Player actor) =>
            !building.Deleted
            && building.CanBeDeletedBy(actor.Faction);

        public override void Execute()
        {
            building.TryRefund();
            building.Delete();
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(building);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<GameObject> building;

        [UsedImplicitly] public Serializer() { }

        public Serializer(GameObject building)
        {
            this.building = building.FindId();
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game.State.Find(building));
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref building);
        }
    }
}

