using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Workers;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("workerHub")]
    sealed class WorkerHub<T> : Component<T, IWorkerHubParameters>
        where T : GameObject, IFactioned
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ImmutableHashSet<WorkerDistributionMethod> upgradableWorkerDistributionMethods =
            ImmutableHashSet.CreateRange(
                new [] {WorkerDistributionMethod.Neutral, WorkerDistributionMethod.RoundRobin});

        private bool canUpgradeToMoreWorkers;
        private ImmutableList<Faction> childFactions;
        private int numWorkersActive;

        public WorkerHub(IWorkerHubParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            canUpgradeToMoreWorkers =
                upgradableWorkerDistributionMethods.Contains(Owner.Game.GameSettings.WorkerDistributionMethod);
            childFactions = Owner.Game.Factions.Where(f => f.Parent == Owner.Faction).ToImmutableList();

            switch (Owner.Game.GameSettings.WorkerDistributionMethod)
            {
                case WorkerDistributionMethod.Neutral:
                case WorkerDistributionMethod.RoundRobin:
                    for (var i = 0; i < Parameters.NumWorkers; i++)
                    {
                        addNewWorker();
                    }
                    break;
                case WorkerDistributionMethod.OnePerPlayer:
                    foreach (var f in childFactions)
                    {
                        addNewWorker(f);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            while (numWorkersActive < Parameters.NumWorkers && canUpgradeToMoreWorkers)
            {
                addNewWorker();
            }
        }

        public override void Draw(GeometryManager geometries) { }

        private void addNewWorker()
        {
            switch (Owner.Game.GameSettings.WorkerDistributionMethod)
            {
                case WorkerDistributionMethod.Neutral:
                    addNewWorker(Owner.Faction);
                    return;
                case WorkerDistributionMethod.RoundRobin:
                    addNewWorker(childFactions[numWorkersActive % childFactions.Count]);
                    return;
                case WorkerDistributionMethod.OnePerPlayer:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void addNewWorker(Faction faction)
        {
            Owner.Game.Add(new Worker(faction.Workers, faction));
            numWorkersActive++;
        }
    }
}
