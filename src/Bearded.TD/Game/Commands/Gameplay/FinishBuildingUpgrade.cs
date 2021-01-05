using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class FinishBuildingUpgrade
    {
        public static ISerializableCommand<GameInstance> Command(BuildingUpgradeTask task) =>
            new Implementation(task);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly BuildingUpgradeTask task;

            public Implementation(BuildingUpgradeTask task)
            {
                this.task = task;
            }

            public void Execute() => task.Complete();

            public ICommandSerializer<GameInstance> Serializer => new Serializer(task);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<BuildingUpgradeTask> task;

            [UsedImplicitly]
            public Serializer() { }

            public Serializer(BuildingUpgradeTask task)
            {
                this.task = task.Id;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
                new Implementation(game.State.Find(task));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref task);
            }
        }
    }
}
