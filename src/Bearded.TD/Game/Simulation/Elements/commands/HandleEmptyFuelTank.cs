using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Elements;

static class HandleEmptyFuelTank
{
    public static ISerializableCommand<GameInstance> Command(GameObject gameObject)
        => new Implementation(gameObject);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject gameObject;

        public Implementation(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public void Execute()
        {
            if (!gameObject.TryGetSingleComponent<IFuelSystem>(out var fuelSystem))
            {
                gameObject.Game.Meta.Logger.Error?.Log($"Could not find fuel system component in {gameObject}");
                return;
            }

            fuelSystem.HandleTankEmpty();
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(gameObject);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> gameObject;

        public Serializer(GameObject gameObject)
        {
            this.gameObject = gameObject.FindId();
        }

        [UsedImplicitly]
        public Serializer() { }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State.Find(gameObject));

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref gameObject);
        }
    }
}
