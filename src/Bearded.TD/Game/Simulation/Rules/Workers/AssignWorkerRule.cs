using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Rules.Workers
{
    [GameRule("assignWorkers")]
    sealed class AssignWorkerRule : GameRule, IListener<WorkerAdded>
    {
        private WorkerDistributionMethod distributionMethod = WorkerDistributionMethod.Neutral;
        private readonly Dictionary<Faction, Queue<Faction>> childFactions = new Dictionary<Faction, Queue<Faction>>();

        protected override void RegisterEvents(GlobalGameEvents events, GameSettings gameSettings)
        {
            base.RegisterEvents(events, gameSettings);

            // TODO: this should use its own internal setting, rather than having a global game settings file
            distributionMethod = gameSettings.WorkerDistributionMethod;
            events.Subscribe(this);
        }

        public void HandleEvent(WorkerAdded @event)
        {
            var hubFaction = @event.Worker.HubOwner.Faction;

            switch (distributionMethod)
            {
                case WorkerDistributionMethod.RoundRobin:
                    if (!childFactions.TryGetValue(hubFaction, out var queue))
                    {
                        queue = new Queue<Faction>();
                        childFactions[hubFaction] = queue;
                        @event.Worker.Game.Factions.Where(f => f.Parent == hubFaction).ForEach(queue.Enqueue);
                    }

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
