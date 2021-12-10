using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay;

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

        public override bool CheckPreconditions(Player actor)
        {
            if (!faction.TryGetBehavior<WorkerTaskManager>(out var factionTaskManager))
            {
                return false;
            }
            actor.Faction.TryGetBehaviorIncludingAncestors<WorkerTaskManager>(out var actorTaskManager);
            return factionTaskManager == actorTaskManager;
        }

        public override void Execute()
        {
            if (!faction.TryGetBehavior<WorkerTaskManager>(out var taskManager))
            {
                throw new InvalidOperationException(
                    "Cannot prioritize task without worker task manager for the faction. " +
                    "Precondition should have failed.");
            }
            taskManager.BumpTaskToTop(workerTask);
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
            var foundFaction = game.State.Factions.Resolve(faction);
            foundFaction.TryGetBehavior<WorkerTaskManager>(out var taskManager);
            // Can safely bypass the null check for the task argument, since preconditions will catch this problem
            // before trying to access the task.
            return new Implementation(foundFaction, taskManager?.TaskFor(workerTask)!);
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
            stream.Serialize(ref workerTask);
        }
    }
}