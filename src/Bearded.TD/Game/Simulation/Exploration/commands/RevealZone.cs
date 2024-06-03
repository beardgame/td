using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Exploration;

static class RevealZone
{
    public static ISerializableCommand<GameInstance> Command(GameState game, Zone zone) =>
        new Implementation(game, zone);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameState game;
        private readonly Zone zone;

        public Implementation(GameState game, Zone zone)
        {
            this.game = game;
            this.zone = zone;
        }

        public void Execute()
        {
            game.VisibilityLayer.RevealZone(zone);
        }

        public ICommandSerializer<GameInstance> Serializer => new Serializer(zone);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<Zone> zone;

        [UsedImplicitly] public Serializer() { }

        public Serializer(Zone zone)
        {
            this.zone = zone.Id;
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref zone);
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
            new Implementation(game.State, game.State.Find(zone));
    }
}
