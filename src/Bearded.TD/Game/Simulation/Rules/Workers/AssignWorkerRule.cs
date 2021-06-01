using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;

namespace Bearded.TD.Game.Simulation.Rules.Workers
{
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
            var hubFaction = @event.Worker.HubOwner.Faction;

            switch (distributionMethod)
            {
                case WorkerDistributionMethod.RoundRobin:
                    var queue = childFactions[hubFaction];
                    if (queue.Count == 0)
                    {
                        @event.Worker.AssignToFaction(hubFaction);
                        break;
                    }

                    var nextFaction = queue.Dequeue();
                    @event.Worker.AssignToFaction(nextFaction);
                    queue.Enqueue(nextFaction);

                    break;
                case WorkerDistributionMethod.Neutral:
                    @event.Worker.AssignToFaction(hubFaction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
