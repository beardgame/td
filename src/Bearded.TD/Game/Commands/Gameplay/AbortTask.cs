using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Workers;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class AbortTask
    {
        public static IRequest<Player, GameInstance> Request(Faction faction, IWorkerTask task)
            => new Implementation(faction, task);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly Faction faction;
            private readonly IWorkerTask task;

            public Implementation(Faction faction, IWorkerTask task)
            {
                this.faction = faction;
                this.task = task;
            }

            public override bool CheckPreconditions(Player actor) =>
                task.CanAbort && faction.SharesWorkersWith(actor.Faction);

            public override void Execute()
            {
                faction.Workers.AbortTask(task);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, task);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private Id<IWorkerTask> task;

            // ReSharper disable once UnusedMember.Local
            public Serializer() {}

            public Serializer(Faction faction, IWorkerTask task)
            {
                this.faction = faction.Id;
                this.task = task.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                var foundFaction = game.State.FactionFor(faction);
                return new Implementation(foundFaction, foundFaction.Workers.TaskFor(task));
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref task);
            }
        }
    }
}
