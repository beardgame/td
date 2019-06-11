using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Workers;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class PrioritizeTask
    {
        public static IRequest<GameInstance> Request(Faction faction, IWorkerTask workerTask)
            => new Implementation(faction, workerTask);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly Faction faction;
            private readonly IWorkerTask workerTask;

            public Implementation(Faction faction, IWorkerTask workerTask)
            {
                this.faction = faction;
                this.workerTask = workerTask;
            }

            public override bool CheckPreconditions() => faction.Workers != null;

            public override void Execute()
            {
                faction.Workers.BumpTaskToTop(workerTask);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, workerTask);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private Id<IWorkerTask> workerTask;

            // ReSharper disable once UnusedMember.Local
            public Serializer() {}

            public Serializer(Faction faction, IWorkerTask workerTask)
            {
                this.faction = faction.Id;
                this.workerTask = workerTask.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                var foundFaction = game.State.FactionFor(faction);
                return new Implementation(foundFaction, foundFaction.Workers.TaskFor(workerTask));
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref workerTask);
            }
        }
    }
}

