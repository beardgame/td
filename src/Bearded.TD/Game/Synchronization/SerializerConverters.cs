using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Commands;
using Bearded.Utilities;

namespace Bearded.TD.Game.Synchronization;

static class SerializerConverters
{
    [SerializerConverter]
    public static readonly ISerializerConverter<GameObject, Id<GameObject>, GameInstance> GameObject =
        new SerializerConverter<GameObject, Id<GameObject>, GameInstance>(
                obj => obj.FindId(),
                (id, game) => game.State.Find(id)
            );
}
