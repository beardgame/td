using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class PrioritizeTask
    {
        public static IRequest<Player, GameInstance> Request(Faction faction, IWorkerTask workerTask)
            => new Implementation(faction, workerTask);

        private sealed class Implementation : UnifiedRequestCommand
        {
            private readonly Faction faction;
            private readonly IWorkerTask workerTask;

            public Implementation(Faction faction, IWorkerTask workerTask)
            {
                this.faction = faction;
                this.workerTask = workerTask;
            }

            public override bool CheckPreconditions(Player actor) => faction.SharesWorkersWith(actor.Faction);

            public override void Execute()
            {
                faction.Workers.BumpTaskToTop(workerTask);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, workerTask);
        }

        private sealed class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private Id<IWorkerTask> workerTask;

            [UsedImplicitly]
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

