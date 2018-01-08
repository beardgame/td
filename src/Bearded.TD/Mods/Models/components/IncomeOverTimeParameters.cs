using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class IncomeOverTimeParameters
    {
        public float IncomePerSecond { get; }

        [JsonConstructor]
        public IncomeOverTimeParameters(float incomePerSecond)
        {
            IncomePerSecond = incomePerSecond;
        }
    }
}
