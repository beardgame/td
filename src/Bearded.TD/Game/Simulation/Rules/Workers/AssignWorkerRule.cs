using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Rules.Workers;

[GameRule("assignWorkers")]
sealed class AssignWorkerRule : GameRule, IListener<WorkerAdded>
{
    private WorkerDistributionMethod distributionMethod = WorkerDistributionMethod.Neutral;
    private readonly Dictionary<Faction, Queue<Faction>> childFactions = new();

    public override void Execute(GameRuleContext context)
    {
        // TODO: this should use its own internal setting, rather than having a global game settings file
        distributionMethod = context.GameSettings.WorkerDistributionMethod;
        context.Events.Subscribe(this);

        var factionsByParent = context.Factions.All.ToLookup(f => f.Parent);
        foreach (var faction in context.Factions.All.Where(f => f.TryGetBehavior<WorkerNetwork>(out _)))
        {
            childFactions[faction] = new Queue<Faction>(factionsByParent[faction]);
        }
    }

    public void HandleEvent(WorkerAdded @event)
    {
        var hubFaction = @event.Worker.Faction;

        switch (distributionMethod)
        {
            case WorkerDistributionMethod.RoundRobin:
                var queue = childFactions[hubFaction];
                if (queue.Count == 0)
                {
                    assignToFaction(@event.Worker, hubFaction);
                    break;
                }

                var nextFaction = queue.Dequeue();
                assignToFaction(@event.Worker, nextFaction);
                queue.Enqueue(nextFaction);

                break;
            case WorkerDistributionMethod.Neutral:
                assignToFaction(@event.Worker, hubFaction);
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    private void assignToFaction(IWorkerComponent worker, Faction faction)
    {
        if (!faction.TryGetBehavior<WorkerTaskManager>(out var taskManager))
        {
            throw new NotSupportedException("Unexpected lack of worker task manager when assigning workers.");
        }
        worker.AssignToTaskManager(taskManager);
    }
}