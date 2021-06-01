using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class AbortTask
    {
        public static IRequest<Player, GameInstance> Request(Faction faction, IWorkerTask task)
            => new Implementation(faction, task);

        private sealed class Implementation : UnifiedRequestCommand
        {
            private readonly Faction faction;
            private readonly IWorkerTask task;

            public Implementation(Faction faction, IWorkerTask task)
            {
                this.faction = faction;
                this.task = task;
            }

            public override bool CheckPreconditions(Player actor)
            {
                if (!faction.TryGetBehavior<WorkerTaskManager>(out var factionTaskManager))
                {
                    return false;
                }
                actor.Faction.TryGetBehaviorIncludingAncestors<WorkerTaskManager>(out var actorTaskManager);
                return factionTaskManager == actorTaskManager && task.CanAbort;
            }

            public override void Execute()
            {
                if (!faction.TryGetBehavior<WorkerTaskManager>(out var taskManager))
                {
                    throw new InvalidOperationException(
                        "Cannot abort task without worker task manager for the faction. " +
                        "Precondition should have failed.");
                }
                taskManager.AbortTask(task);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, task);
        }

        private sealed class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private Id<IWorkerTask> task;

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(Faction faction, IWorkerTask task)
            {
                this.faction = faction.Id;
                this.task = task.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                var foundFaction = game.State.Factions.Resolve(faction);
                foundFaction.TryGetBehavior<WorkerTaskManager>(out var taskManager);
                // Can safely bypass the null check for the task argument, since preconditions will catch this problem
                // before trying to access the task.
                return new Implementation(foundFaction, taskManager?.TaskFor(task)!);
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref task);
            }
        }
    }
}
