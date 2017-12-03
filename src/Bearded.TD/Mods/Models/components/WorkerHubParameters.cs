using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class WorkerHubParameters
    {
        public int NumWorkers { get; }

        [JsonConstructor]
        public WorkerHubParameters(int numWorkers)
        {
            NumWorkers = numWorkers;
        }
    }
}
