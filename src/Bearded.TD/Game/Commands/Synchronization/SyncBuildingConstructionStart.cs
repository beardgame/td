using System;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.Synchronization
{
    static class SyncBuildingConstructionStart
    {
        public static ISerializableCommand<GameInstance> Command(ComponentGameObject building)
            => new Implementation(building);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly ComponentGameObject building;

            public Implementation(ComponentGameObject building)
            {
                this.building = building;
            }

            public void Execute()
            {
                var constructor = building.GetComponents<IBuildingConstructionSyncer>().SingleOrDefault()
                    ?? throw new InvalidOperationException(
                        "Cannot sync building construction start on a building without construction syncer.");
                constructor.SyncStartBuild();
            }

            ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(building);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<ComponentGameObject> building;

            public Serializer(ComponentGameObject building)
            {
                this.building = building.FindId();
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
                new Implementation(game.State.Find(building));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref building);
            }
        }
    }
}

